using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerParentPlayerCommandService(
    IPlayerServiceClient playerServiceClient,
    ITgsRepository repository) : IGetService<SetPlayerParentPlayerCommand, SetPlayerParentPlayerInfo>
{
    private readonly IPlayerServiceClient _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    private readonly ITgsRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async ValueTask<SetPlayerParentPlayerInfo> GetAsync(SetPlayerParentPlayerCommand query, CancellationToken cancellationToken = default)
    {
        var player = await _playerServiceClient
            .GetPlayerInfoAsync(query.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerParentPlayerInfo(false, Application.Resources.Player.NotFound);

        var parentPlayerInfo = await _playerServiceClient
            .GetPlayerInfoByUserNameAsync(player.BrandId, query.ParentUsername, cancellationToken)
            .ConfigureAwait(false);

        var parentPlayer = await _repository.PlayerParents
            .FirstOrDefaultAsync(pp => pp.PlayerCode == player.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (parentPlayer is not null)
        {
            parentPlayer.ParentPlayerCode = parentPlayerInfo.PlayerCode;
        }
        else
        {
            _ = await _repository.PlayerParents.AddAsync(new Core.DataServices.Tgs.Models.PlayerParent
            {
                PlayerCode = player.PlayerCode,
                ParentPlayerCode = parentPlayerInfo.PlayerCode,
            }, cancellationToken).ConfigureAwait(false);
        }

        var auditLog = new PlayerAuditLog
        {
            Id = Guid.NewGuid(),
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

        return new SetPlayerParentPlayerInfo(true, string.Empty);
    }
}
