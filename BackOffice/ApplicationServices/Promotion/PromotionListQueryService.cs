using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;

using PromotionStatus = Tgs.Promotion.Clients.PromotionStatus;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion;

internal class PromotionListQueryService(
    IPromotionServiceClient promotionServiceClient) : IQueryService<PromotionListQuery, PromotionView>
{
    private readonly IPromotionServiceClient _promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));

    public IAsyncEnumerable<PromotionView> QueryAsync(
        PromotionListQuery query,
        CancellationToken cancellationToken = default)
    {
        var status = query.Status is null
            ? (PromotionStatus?)null
            : query.Status switch
            {
                SwitchStatus.Enabled => PromotionStatus.Enabled,
                SwitchStatus.Disabled => PromotionStatus.Disabled,
                _ => throw new NotImplementedException()
            };

        return _promotionServiceClient.ListPromotionsAsync(
            query.BrandId,
            query.EventDuring,
            query.Name,
            query.PromotionTypes,
            status,
            cancellationToken);
    }
}