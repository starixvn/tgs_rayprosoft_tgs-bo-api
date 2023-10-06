#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.PlayerService.Client;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using PlayerStatus = TGS.Core.Models.Enums.PlayerStatus;

namespace TGS.Services.BackOffice.Application.Commands.PlayerHandlers;

public class UpdatePlayerHandler : Handler<UpdatePlayerRequest, MessageResponse>
{
    private readonly IAgentService _agentService;
    private readonly IGameProvidersBackofficePluggableClient _gameProvidersBackofficePluggableClient;
    private readonly ILogger<UpdatePlayerHandler> _logger;
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly IPlayerService _service;

    public UpdatePlayerHandler(
        ILogger<UpdatePlayerHandler> logger,
        IPlayerService service,
        IAgentService agentService,
        IHttpContextAccessor accessor,
        IGameProvidersBackofficePluggableClient gameProvidersBackofficePluggableClient,
        IPlayerServiceClient playerServiceClient)
        : base(accessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service;
        _agentService = agentService;
        _gameProvidersBackofficePluggableClient = gameProvidersBackofficePluggableClient ?? throw new ArgumentNullException(nameof(gameProvidersBackofficePluggableClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public override async Task<MessageResponse> Handle(
        UpdatePlayerRequest request,
        CancellationToken cancellationToken)
    {
        var updatePlayerType = new[]
        {
            "info",
            "contact",
            "password"
        };

        if (!updatePlayerType.Contains(request.Type))
        {
            return CreateInvalidResponse<MessageResponse>(Player.UpdateTypeNotFound);
        }

        var player = await _service.GetPlayerAsync(p => p.PlayerCode == request.PlayerCode)
            .ConfigureAwait(false);

        if (player == null)
        {
            return CreateInvalidResponse<MessageResponse>(Player.NotFound);
        }

        if (!request.AgentUsername.IsNullOrEmpty())
        {
            var agent = await _agentService
                .GetAgentAsync(p => p.BrandId == player.BrandId
                    && p.Status == AgentStatus.Enabled
                    && p.Username == request.AgentUsername)
                .ConfigureAwait(false);

            if (agent == null)
            {
                return CreateInvalidResponse<MessageResponse>(Agent.NotFound);
            }

            player.AgentId = agent.Id;
        }

        if (!request.ParentUsername.IsNullOrEmpty() && request.Type == updatePlayerType[0])
        {
            var parentPlayer = await _service
                .GetPlayerAsync(p => p.Username == request.ParentUsername && p.BrandId == player.BrandId)
                .ConfigureAwait(false);

            await _service
                .UpdateParentPlayerAsync(player.PlayerCode, parentPlayer.PlayerCode)
                .ConfigureAwait(false);
        }

        if (!string.IsNullOrEmpty(request.Email) && request.Type == updatePlayerType[1] &&
            await _service.ExistsAsync(x
                => x.BrandId == player.BrandId && x.PlayerCode != player.PlayerCode &&
                x.Email!.ToLower() == request.Email.ToLower()))
        {
            return CreateInvalidResponse<MessageResponse>(Player.BrandEmailDuplicate);
        }

        if (player.Phone.IsNotNullOrEmpty() && request.Phone.IsNullOrEmpty() && request.Type == updatePlayerType[1])
        {
            return CreateInvalidResponse<MessageResponse>(Player.PhoneRequired);
        }

        var kickPlayer = false;
        if (request.Type == updatePlayerType[0] &&
            player.Status != request.Status)
        {
            kickPlayer = true;
        }

        await _service.UpdatePlayerAsync(player, request).ConfigureAwait(false);

        if (request.Type == updatePlayerType[0] && request.Birthdate != null)
        {
            var properties = new Dictionary<PlayerPropertyName, string?>
            {
                [PlayerPropertyName.Birthdate] = request.Birthdate.ToString()
            };

            await _playerServiceClient
                .SetPlayerPropertiesAsync(player.PlayerCode, properties.ToImmutableDictionary(), UserId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (request.Type == updatePlayerType[1])
        {
            var properties = new Dictionary<PlayerPropertyName, string?>
            {
                [PlayerPropertyName.Line] = request.Line,
                [PlayerPropertyName.Viber] = request.Viber,
                [PlayerPropertyName.Telegram] = request.Telegram,
                [PlayerPropertyName.FacebookMessenger] = request.FacebookMessenger,
                [PlayerPropertyName.WhatsApp] = request.WhatsApp
            };

            await _playerServiceClient.SetPlayerPropertiesAsync(
                player.PlayerCode,
                properties.ToImmutableDictionary(),
                modifyUser: UserId,
                cancellationToken)
                .ConfigureAwait(false);
        }

        if (kickPlayer)
        {
            if (request.Status is PlayerStatus.Disabled or PlayerStatus.InBlacklist)
            {
                _logger.LogInformation("call KickPlayerAsync");
                await _gameProvidersBackofficePluggableClient
                    .KickPlayerAsync(request.PlayerCode, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return CreateValidResponse<MessageResponse>();
    }
}
