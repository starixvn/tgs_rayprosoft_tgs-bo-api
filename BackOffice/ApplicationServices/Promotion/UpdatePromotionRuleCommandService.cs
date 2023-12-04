using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class UpdatePromotionRuleCommandService(
    IPromotionServiceClient promotionServiceClient) : ICommandService<UpdatePromotionRuleCommand>
{
    private readonly IPromotionServiceClient _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));

    public async ValueTask ExecuteAsync(
        UpdatePromotionRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        await _promotionServiceClient.SetPromotionRuleAsync(
            command.PromotionId,
            command.Rule,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);

        await _promotionServiceClient.SetPromotionVipLevelsAsync(
            command.PromotionId,
            command.VipLevels,
            command.Modifier,
            cancellationToken).ConfigureAwait(false);
    }
}