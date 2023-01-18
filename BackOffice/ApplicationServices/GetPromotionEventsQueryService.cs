using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetPromotionEventsQueryService : IQueryService<GetPromotionEventsQuery, PromotionEventMainInfo>
{
    private readonly IPromotionHistoryClient _promotionHistoryClient;

    public GetPromotionEventsQueryService(
        IPromotionHistoryClient promotionHistoryClient)
    {
        _promotionHistoryClient = promotionHistoryClient ?? throw new ArgumentNullException(nameof(promotionHistoryClient));
    }

    public IAsyncEnumerable<PromotionEventMainInfo> QueryAsync(
        GetPromotionEventsQuery query,
        CancellationToken cancellationToken = default)
        => _promotionHistoryClient
            .QueryPromotionEventsAsync(query.PromotionId, cancellationToken);
}
