#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class GetPromotionEventDetailQueryService(IPromotionHistoryClient promotionHistoryClient) : IGetService<GetPromotionEventDetailQuery, PromotionEventDetailInfo?>
{
    private readonly IPromotionHistoryClient _promotionHistoryClient = promotionHistoryClient ?? throw new ArgumentNullException(nameof(promotionHistoryClient));

    public ValueTask<PromotionEventDetailInfo?> GetAsync(
        GetPromotionEventDetailQuery query,
        CancellationToken cancellationToken = default)
        => _promotionHistoryClient.GetPromotionEventDetailAsync(
            query.PromotionId,
            query.EventId,
            cancellationToken);
}
