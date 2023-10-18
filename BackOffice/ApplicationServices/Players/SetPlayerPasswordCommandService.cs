using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerPasswordCommandService : IGetService<SetPlayerPasswordCommand, SetPlayerPasswordInfo>
{
    private readonly IPlayerServiceClient _playerServiceClient;
    private readonly ITgsRepository _tgsRepository;

    public SetPlayerPasswordCommandService(
        IPlayerServiceClient playerServiceClient,
        ITgsRepository tgsRepository)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public async ValueTask<SetPlayerPasswordInfo> GetAsync(SetPlayerPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var player = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerPasswordInfo(false, Application.Resources.Player.NotFound);

        await _playerServiceClient
            .SetPlayerPasswordAsync(command.PlayerCode, command.Password, command.ModifyUsername, command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        var auditLog = new PlayerAuditLog
        {
            Id = GuidExtensions.NewSequentialGuid(),
            ModuleName = "Player",
            CreatedByUsername = command.ModifyUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = "Update Player password",
            Username = command.PlayerCode
        };

        _ = await _tgsRepository.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await _tgsRepository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SetPlayerPasswordInfo(true, string.Empty);
    }
}
