#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/promotion/history")]
[ApiController]
public class PromotionHistoryController : ControllerBase
{
    [HttpGet("{promotionId}", Name = "QueryPromotionEvents")]
    public IAsyncEnumerable<PromotionEventViewModel> QueryPromotionEventsAsync(
        [FromServices] IQueryService<GetPromotionEventsQuery, PromotionEventMainInfo> queryService,
        Guid promotionId)
    {
        var query = new GetPromotionEventsQuery(promotionId);

        return queryService.QueryAsync(
                query, 
                HttpContext.RequestAborted)
            .Select(evt => new PromotionEventViewModel
            {
                EventId = evt.EventId,
                Action = evt.Action,
                PromotionId = evt.PromotionId,
                CreatedOn = evt.CreatedOn,
                Modifier = evt.Modifier
            });
    }

    [HttpGet("detail/{promotionId}/{eventId}", Name = "GetPromotionEventDetail")]
    public async ValueTask<PromotionEventDetailViewModel?> GetPromotionEventDetailAsync(
        [FromServices] IGetService<GetPromotionEventDetailQuery, PromotionEventDetailInfo?> getService,
        Guid promotionId,
        Guid eventId)
    {
        var query = new GetPromotionEventDetailQuery(promotionId, eventId);

        var info = await getService.GetAsync(
                query,
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return info is null
            ? null
            : new PromotionEventDetailViewModel
            {
                EventId = info.EventId,
                PromotionId = info.PromotionId,
                Action = info.Action,
                CreatedOn = info.CreatedOn,
                ChangeSets =  info.ChangeSets,
                Modifier = info.Modifier
            };
    }
}