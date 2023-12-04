#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class GetPromotionMainInfoQueryService(
    IPromotionServiceClient promotionServiceClient) : IGetService<GetPromotionMainInfoQuery, PromotionMainInfo?>
{
    private readonly IPromotionServiceClient _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));

    public ValueTask<PromotionMainInfo?> GetAsync(
        GetPromotionMainInfoQuery query,
        CancellationToken cancellationToken = default)
        => _promotionServiceClient.GetPromotionMainInfoAsync(query.PromotionId, cancellationToken);
}