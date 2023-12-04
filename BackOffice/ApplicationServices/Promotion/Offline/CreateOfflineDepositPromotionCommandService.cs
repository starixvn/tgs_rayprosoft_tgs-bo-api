using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class CreateOfflineDepositPromotionCommandService(IPromotionServiceClient promotionServiceClient) : ICommandService<CreateOfflineDepositPromotionCommand>
{
    private readonly IPromotionServiceClient _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));

    public async ValueTask ExecuteAsync(CreateOfflineDepositPromotionCommand command, CancellationToken cancellationToken = default)
    {
        var createInfo = new CreatePromotionMainInfo
        {
            Name = command.PromotionName,
            EventDuring = command.EventDuring,
            PromotionType = "OfflineDepositReward",
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
            command.DepositSettings,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            PromotionStatus.Enabled,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);
    }
}
