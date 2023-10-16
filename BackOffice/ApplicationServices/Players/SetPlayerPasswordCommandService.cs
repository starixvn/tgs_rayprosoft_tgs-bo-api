using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerPasswordCommandService : ICommandService<SetPlayerPasswordCommand>
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

    public async ValueTask ExecuteAsync(SetPlayerPasswordCommand command, CancellationToken cancellationToken = default)
    {
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
    }
}
