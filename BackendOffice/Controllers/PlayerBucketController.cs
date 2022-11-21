using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/player/bucket")]
[ApiController]
public class PlayerBucketController : BaseContextController
{
    [HttpGet("amount/{playerCode}", Name = "GetPlayerBucketAmount2")]
    public async Task<decimal> GetPlayerBucketAmountAsync(
        [FromServices] IGetService<GetPlayerBucketAmountQuery, ReadBucketResult> getService,
        string playerCode)
    {
        var query = new GetPlayerBucketAmountQuery(playerCode);

        var result = await getService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return result.Amount;
    }

    [HttpGet("history/{bucketId}", Name = "GetPlayerBucketHistories2")]
    public IAsyncEnumerable<PlayerBucketEventViewModel> GetPlayerBucketHistoriesAsync(
        [FromServices] IQueryService<GetPlayerBucketHistoriesQuery, BucketEvent> queryService,
        Guid bucketId)
    {
        var query = new GetPlayerBucketHistoriesQuery(bucketId);

        return queryService
            .QueryAsync(query, HttpContext.RequestAborted)
            .Select(evt => new PlayerBucketEventViewModel
            {
                Id = evt.Id,
                BucketId = evt.BucketId,
                ChangeAmount = evt.ChangeAmount,
                Type = evt.Type,
                ExternalId = evt.ExternalId,
                CreatedOn = evt.CreatedOn
            });
    }

    [HttpGet("{playerCode}", Name = "GetPlayerBuckets2")]
    public IAsyncEnumerable<PlayerBucketViewModel> GetPlayerBucketsAsync(
        [FromServices] IQueryService<GetPlayerBucketsQuery, BucketInfo> queryService,
        string playerCode)
    {
        var query = new GetPlayerBucketsQuery(playerCode);

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Select(info => new PlayerBucketViewModel
            {
                Id = info.Id,
                BucketType = info.Type,
                PlayerCode = info.PlayerCode,
                WageringRequirement = info.Amount,
                UncompleteWagering = info.FillInAmount,
                CreatedOn = info.CreatedOn
            });
    }
}
