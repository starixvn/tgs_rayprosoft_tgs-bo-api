using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;
using Tgs.VipLevelService.Clients;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class SetPlayerVipLevelCommandService(
    ILogger<SetPlayerVipLevelCommandService> logger,
    IVipLevelServiceClient vipLevelServiceClient) : ICommandService<SetPlayerVipLevelCommand>
{
    public async ValueTask ExecuteAsync(SetPlayerVipLevelCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("LogOn:{logOn} Mesaage:This command information.({command})",
            DateTime.Now.ToString("o"),
            command);

        var playerVipLevelInfo = await vipLevelServiceClient.PlayerVipLevelGetAsync(
            playerCode: command.PlayerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        logger.LogDebug("LogOn:{logOn} Mesaage:This player’s VIP information.({playerVipLevelInfo})",
            DateTime.Now.ToString("o"),
            playerVipLevelInfo);

        var vipLevelInfo = await vipLevelServiceClient.VipLevelGetAsync(
            id: command.VipId,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NullReferenceException($"The VIP level for this brand does not exist.({command.VipId})");

        logger.LogDebug("LogOn:{logOn} Mesaage:Vip Level Info.({vipLevelInfo})",
            DateTime.Now.ToString("o"),
            vipLevelInfo);

        if (playerVipLevelInfo.VipLevel != vipLevelInfo.Level ||
            playerVipLevelInfo.AutoState != command.AutoState)
        {
            logger.LogDebug("LogOn:{logOn} Mesaage:This adjusts VipLevl.", DateTime.Now.ToString("o"));
            await vipLevelServiceClient.PlayerVipLevelManualAsync(
                playerCode: playerVipLevelInfo.PlayerCode,
                vipLevel: vipLevelInfo.Level,
                autoState: command.AutoState,
                modifierUserId: command.ModifyUserId,
                modifierUsername: command.ModifyUsername,
                remark: command.Remark,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
