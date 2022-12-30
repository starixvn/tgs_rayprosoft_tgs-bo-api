using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;

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
}