using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class CreateOfflineLoginPromotionCommandService(IPromotionServiceClient promotionServiceClient) : ICommandService<CreateOfflineLoginPromotionCommand>
{
    private readonly IPromotionServiceClient _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));

    public async ValueTask ExecuteAsync(CreateOfflineLoginPromotionCommand command, CancellationToken cancellationToken = default)
    {
        var createInfo = new CreatePromotionMainInfo
        {
            Name = command.PromotionName,
            EventDuring = command.EventDuring,
            PromotionType = "OfflineLoginReward",
            IsAutoSent = true,
            PromotionLimitAmount = null
        };

        var promotionId = await _promotionServiceClient.CreatePromotionAsync(
            command.BrandId,
            createInfo,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _promotionServiceClient.SetPromotionRuleAsync(
            promotionId,
            command.LoginSettings,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            PromotionStatus.Enabled,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);
    }
}
