#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class GetPromotionRuleQueryService : IGetService<GetPromotionRuleQuery, PromotionRuleInfo?>
{
    private readonly IPromotionServiceClient _promotionServiceClient;

    public GetPromotionRuleQueryService(IPromotionServiceClient promotionServiceClient)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
    }

    public ValueTask<PromotionRuleInfo?> GetAsync(GetPromotionRuleQuery query, CancellationToken cancellationToken = default)
        => _promotionServiceClient.GetPromotionRuleAsync<PromotionRuleInfo>(query.PromotionId, cancellationToken);
}
