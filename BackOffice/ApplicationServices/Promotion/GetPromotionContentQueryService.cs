#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class GetPromotionContentQueryService : IGetService<GetPromotionContentQuery, PromotionContent?>
{
    private readonly IPromotionServiceClient _promotionServiceClient;

    public GetPromotionContentQueryService(
        IPromotionServiceClient promotionServiceClient)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
    }

    public ValueTask<PromotionContent?> GetAsync(
        GetPromotionContentQuery query,
        CancellationToken cancellationToken = default) 
        => _promotionServiceClient.GetPromotionContentAsync(query.PromotionId, cancellationToken);
}
