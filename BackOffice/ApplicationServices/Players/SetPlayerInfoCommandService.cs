#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerInfoCommandService(
    IPlayerServiceClient playerServiceClient,
    IVipLevelServiceClient vipLevelServiceClient,
    ICommandService<SetPlayerVipLevelCommand> setPlayerVipLevelService,
    ITgsRepository repository) : IGetService<SetPlayerInfoCommand, SetPlayerInfoResult>
{
    public async ValueTask<SetPlayerInfoResult> GetAsync(SetPlayerInfoCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.ModifyInfo.Any())
            return new SetPlayerInfoResult(false, string.Empty);

        var player = await playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        if (player is null)
            return new SetPlayerInfoResult(false, Application.Resources.Player.NotFound);

        var availablePropertyNames = new HashSet<string>(
            new string[]
            {
                nameof(PlayerPropertyName.RealName),
                nameof(PlayerPropertyName.Birthdate),
                nameof(PlayerPropertyName.Remark),
                nameof(PlayerPropertyName.VipId),
                nameof(PlayerPropertyName.IdNumber),
                nameof(PlayerPropertyName.VipLevelAutoState),
            },
            StringComparer.OrdinalIgnoreCase);

        var modifyDict = new Dictionary<PlayerPropertyName, string?>();

        foreach (var item in command.ModifyInfo)
        {
            if (availablePropertyNames.Contains(item.Key))
            {
                modifyDict.Add(Enum.Parse<PlayerPropertyName>(item.Key, true), item.Value);
            }
        }

        var vipLevelAutoStateString = string.Empty;

        if (modifyDict.TryGetValue(PlayerPropertyName.VipId, out var vipIdString)
            || modifyDict.TryGetValue(PlayerPropertyName.VipLevelAutoState, out vipLevelAutoStateString))
        {
            var playerVipLevelInfo = await vipLevelServiceClient.PlayerVipLevelGetAsync(
                playerCode: player.PlayerCode,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var vipId = playerVipLevelInfo.VipLevelId;
            var autoState = playerVipLevelInfo.AutoState;

            if (!string.IsNullOrWhiteSpace(vipIdString) && Guid.TryParse(vipIdString, out _))
            {
                vipId = Guid.Parse(vipIdString);
            }

            if (!string.IsNullOrWhiteSpace(vipLevelAutoStateString) && Enum.TryParse(vipLevelAutoStateString, out Tgs.VipLevelService.Common.PlayerVipLevelAutoState _))
            {
                autoState = (Tgs.VipLevelService.Common.PlayerVipLevelAutoState)Enum.Parse(typeof(Tgs.VipLevelService.Common.PlayerVipLevelAutoState), vipLevelAutoStateString, true);
            }

            if (vipId != playerVipLevelInfo.VipLevelId || autoState != playerVipLevelInfo.AutoState)
            {
                var setPlayerVipLevelCommand = new SetPlayerVipLevelCommand(
                    PlayerCode: command.PlayerCode,
                    ModifyUserId: Guid.Parse(command.ModifyUserId),
                    ModifyUsername: command.ModifyUsername,
                    VipId: vipId,
                    AutoState: autoState,
                    Remark: string.Empty);
                await setPlayerVipLevelService.ExecuteAsync(
                    command: setPlayerVipLevelCommand,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        await playerServiceClient
            .SetPlayerPropertiesAsync(command.PlayerCode, modifyDict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        var auditLog = new PlayerAuditLog
        {
            Id = Guid.NewGuid(),
            ModuleName = "Player",
            CreatedByUsername = command.ModifyUsername,
            CreatedOn = DateTimeOffset.Now,
            Operation = "Update Player info",
            Username = command.PlayerCode
        };

        _ = await repository.PlayerAuditLogs
            .AddAsync(auditLog, cancellationToken)
            .ConfigureAwait(false);

        _ = await repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return new SetPlayerInfoResult(true, string.Empty);
    }
}
