#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;
using Tgs.VipLevelService.Common;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerInfoCommandService(
    IPlayerServiceClient playerServiceClient,
    IVipLevelServiceClient vipLevelServiceClient,
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

        await playerServiceClient
            .SetPlayerPropertiesAsync(command.PlayerCode, modifyDict.ToImmutableDictionary(), command.ModifyUserId, cancellationToken)
            .ConfigureAwait(false);

        if (modifyDict.TryGetValue(PlayerPropertyName.VipId, out var vipIdString))
        {
            if (!string.IsNullOrEmpty(vipIdString) && Guid.TryParse(vipIdString, out var vipId))
            {
                var playerVipLevelInfo = await vipLevelServiceClient.PlayerVipLevelGetAsync(
                    playerCode: command.PlayerCode,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var vipLevelInfo = await vipLevelServiceClient.VipLevelGetAsync(
                    vipId,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false)
                    ?? throw new NullReferenceException($"The VIP level for this brand does not exist.({vipId})");

                if (playerVipLevelInfo.VipLevel != vipLevelInfo.Level)
                {
                    await vipLevelServiceClient.PlayerVipLevelManualAsync(
                        playerCode: playerVipLevelInfo.PlayerCode,
                        vipLevel: vipLevelInfo.Level,
                        autoState: playerVipLevelInfo.AutoState,
                        modifierUserId: Guid.Parse(command.ModifyUserId),
                        modifierUsername: command.ModifyUsername,
                        remark: string.Empty,
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }

        var auditLog = new PlayerAuditLog
        {
            Id = GuidExtensions.NewSequentialGuid(),
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
