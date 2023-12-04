using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerStatusCommandService(
    IPlayerServiceClient playerServiceClient,
    IGameProvidersBackofficePluggableClient pluggableClient,
    ITgsRepository repository,
    ILogger<SetPlayerStatusCommandService> logger) : IGetService<SetPlayerStatusCommand, SetPlayerStatusResult>
{
    private readonly ILogger<SetPlayerStatusCommandService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    private readonly IGameProvidersBackofficePluggableClient _pluggableClient = pluggableClient ?? throw new ArgumentNullException(nameof(pluggableClient));
    private readonly ITgsRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async ValueTask<SetPlayerStatusResult> GetAsync(SetPlayerStatusCommand query, CancellationToken cancellationToken = default)
    {
        var player = await _playerServiceClient
            .GetPlayerInfoAsync(query.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerStatusResult(false, Application.Resources.Player.NotFound);

        var playerStatus = await _playerServiceClient
            .GetPlayerStatusAsync(query.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        await _playerServiceClient
            .SetPlayerStatusAsync(query.PlayerCode, (PlayerStatus)query.Status, query.ModfiyUserId, cancellationToken)
            .ConfigureAwait(false);

        if (playerStatus != (PlayerStatus)query.Status
            && query.Status is Core.Models.Enums.PlayerStatus.Disabled or Core.Models.Enums.PlayerStatus.InBlacklist)
        {
            _logger.LogInformation("call KickPlayerAsync");
            await _pluggableClient
                .KickPlayerAsync(query.PlayerCode, cancellationToken)
                .ConfigureAwait(false);
        }

        var auditLog = new PlayerAuditLog
        {
            Id = GuidExtensions.NewSequentialGuid(),
            ModuleName = "Player",
            CreatedByUsername = query.ModifyUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = "Update Player info",
            Username = query.PlayerCode
        };

        _ = await _repository.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SetPlayerStatusResult(true, string.Empty);
    }
}
