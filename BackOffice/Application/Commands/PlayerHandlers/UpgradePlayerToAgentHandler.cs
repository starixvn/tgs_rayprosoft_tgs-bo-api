#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using TGS.Core.Models.Messages;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;
using AgentLevelType = Tgs.Agent.Client.Models.Enums.AgentLevelType;
using AgentType = Tgs.Agent.Client.Models.Enums.AgentType;
using System;

namespace TGS.Services.BackOffice.Application.Commands.PlayerHandlers;

public class UpgradePlayerToAgentHandler(
    IPlayerService service,
    IAgentClient agentClient,
    IPlayerServiceClient playerServiceClient,
    IHttpContextAccessor accessor)
    : Handler<UpgradePlayerToAgentRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(
        UpgradePlayerToAgentRequest request,
        CancellationToken cancellationToken)
    {
        var player = await service.GetPlayerAsync(x => x.PlayerCode == request.PlayerCode);

        var playerProperties =
            await playerServiceClient.GetPlayerPropertiesAsync(request.PlayerCode, cancellationToken)
                .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProperties["DefaultWalletId"]);

        var agentLevel = player.AgentId == null ? AgentLevelType.Chief : AgentLevelType.Agent;

        var agentId = await agentClient.CreateAgentByPlayerUpgradeAsync(new CreateAgentByPlayerUpgradeRequest(
                player.BrandId,
                playerWalletId,
                player.Password,
                player.Salt,
                player.Username,
                player.RealName,
                player.PlayerCode,
                player.DefaultWalletCurrencyCode,
                player.Phone,
                player.Email,
                request.EditorUsername,
                agentLevel,
                AgentType.Cash,
                player.AgentId,
                "player upgrade"),
            cancellationToken);

        await playerServiceClient.SetPlayerPropertiesAsync(player.PlayerCode,
            new Dictionary<PlayerPropertyName, string?> { { PlayerPropertyName.AgentId, agentId.ToString() } }
                .ToImmutableDictionary(),
            request.EditorUsername,
            cancellationToken);

        return CreateValidResponse<MessageResponse>();
    }
}
