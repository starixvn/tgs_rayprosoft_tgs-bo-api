using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;

using PromotionStatus = Tgs.Promotion.Clients.PromotionStatus;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class PromotionListQueryService : IQueryService<PromotionListQuery, PromotionView>
{
    private readonly IPromotionServiceClient _promotionServiceClient;

    public PromotionListQueryService(
        IPromotionServiceClient promotionServiceClient)
    {
        _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
    }

    public IAsyncEnumerable<PromotionView> QueryAsync(
        PromotionListQuery query, 
        CancellationToken cancellationToken = default)
    {
        During? eventDuring = null;

        if (query.From.HasValue && query.To.HasValue)
        {
            var offset = TimeSpan.FromHours(8D);
            var from = new DateTimeOffset(query.From.Value.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;
            var to = new DateTimeOffset(query.To.Value.ToDateTime(TimeOnly.MinValue), offset).UtcDateTime;

            eventDuring = (from, to);
        }

        var status = query.Status is null
            ?　(PromotionStatus?)null 
            : query.Status switch
            {
                SwitchStatus.Enabled => PromotionStatus.Enabled,
                SwitchStatus.Disabled => PromotionStatus.Disabled,
                _ => throw new NotImplementedException()
            };

        return _promotionServiceClient.ListPromotionsAsync(
            query.BrandId,
            eventDuring,
            query.Name,
            query.PromotionType, 
            status,
            cancellationToken);
    }
}