using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tgs.DepositAccumulateRoulette.Clients;
using Tgs.Paging;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Clients.Models.Promotion;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.DepositAccumulateRoulette;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/dar")]
[ApiController]
public class DepositAccumulateRouletteController : BaseContextController
{
    [HttpGet("detail/{brandId}/{recordId}", Name = "GetDepositAccumulateRouletteGameRecordDetail")]
    public async ValueTask<GenericResponse<DepositAccumulateRouletteGameRecordDetailViewModel>> GetDepositAccumulateRouletteGameRecordDetailAsync(
        [FromServices] IGetService<DepositAccumulateRouletteRecordDetailQuery, DepositAccumulateRouletteGameRecordDetailInfo> getService,
        Guid brandId,
        Guid recordId)
    {
        var detailInfo = await getService.GetAsync(
                new DepositAccumulateRouletteRecordDetailQuery(brandId, recordId),
                HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new GenericResponse<DepositAccumulateRouletteGameRecordDetailViewModel>
        {
            Valid = true,
            Data = new DepositAccumulateRouletteGameRecordDetailViewModel
            {
                WinningPrize = detailInfo.WinningPrize
            }
        };
    }

    [HttpGet("records/{brandId}", Name = "QueryDepositAccumulateRouletteGameRecords")]
    public async ValueTask<DepositAccumulateRouletteGameRecordsResponse> QueryDepositAccumulateRouletteGameRecordsAsync(
        [FromServices] IPlayerServiceClient playerServiceClient,
        [FromServices] IPromotionClient promotionClient,
        [FromServices] IPromotionServiceClient promotionServiceClient,
        [FromServices] IQueryService<DepositAccumulateRouletteGameRecordQuery, RouletteRecord> queryService,
        Guid brandId,
        [FromQuery] DepositAccumulateRouletteGameRecordsRequest request,
        [FromQuery] PageableRequest pagingRequest)
    {
        (DateTimeOffset Begin, DateTimeOffset End) timeRange;

        if (request.From != null && request.To != null)
            timeRange = (request.From.Value, request.To.Value);
        else
        {
            var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D));
            timeRange = (now.AddDays(-3D), now);
        }

        var playerCodes = Array.Empty<string>();

        if (!string.IsNullOrEmpty(request.UserName))
        {
            var player = await playerServiceClient.GetPlayerInfoByUserNameAsync(
                brandId,
                request.UserName,
                HttpContext.RequestAborted).ConfigureAwait(false);

            if (player is null)
                return new DepositAccumulateRouletteGameRecordsResponse
                {
                    Valid = true,
                    TotalCount = 0,
                    Items = []
                };
            else
                playerCodes = [player.PlayerCode];
        }

        var promotionIds = Array.Empty<Guid>();

        if (!string.IsNullOrEmpty(request.PromotionName))
        {
            promotionIds = await promotionServiceClient.QueryPromotionIdsAsync(
                    brandId,
                    request.PromotionName,
                    "DepositAccumulateRoulette",
                    HttpContext.RequestAborted)
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false);

            if (promotionIds.Length == 0)
            {
                return new DepositAccumulateRouletteGameRecordsResponse
                {
                    Valid = true,
                    TotalCount = 0,
                    Items = []
                };
            }
        }

        var paging = new Paging<RouletteRecord>();

        if (!string.IsNullOrEmpty(pagingRequest.SortBy))
            paging.SortByPropertyName(pagingRequest.SortBy, pagingRequest.IsDesc);

        paging.RowCount = pagingRequest.Rows;
        paging.Page = pagingRequest.Page;

        var query = new DepositAccumulateRouletteGameRecordQuery(
            brandId,
            promotionIds,
            playerCodes,
            timeRange);

        var result = queryService
            .QueryAsync(query, HttpContext.RequestAborted);

        var totalCount = await result.CountAsync(HttpContext.RequestAborted).ConfigureAwait(false);

        var rouletteRecords = await result
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var dataQuery = await rouletteRecords
            .ToAsyncEnumerable()
            .ApplyOrderBy(paging)
            .ApplyPaging(paging)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var playerUserNames = dataQuery.Length > 0
            ? await playerServiceClient.QueryPlayerInfoListAsync(
                brandId,
                dataQuery
                    .Select(r => r.PlayerCode)
                    .Distinct(),
                HttpContext.RequestAborted)
                .ToDictionaryAsync(
                    info => info.PlayerCode,
                    info => info.UserName,
                    HttpContext.RequestAborted)
                .ConfigureAwait(false)
            : new Dictionary<string, string>(0);

        var promotionNames = dataQuery.Length > 0
            ? (await promotionClient
                .GetPromotionNamesAsync(new GetPromotionNamesRequest(
                    dataQuery
                        .Select(info => info.PromotionId)
                        .Distinct()
                        .ToArray()))
                .ConfigureAwait(false))
                .ToDictionary(model => model.Id, model => model.Name)
            : new Dictionary<Guid, string>(0);

        return new DepositAccumulateRouletteGameRecordsResponse
        {
            Valid = true,
            TotalCount = totalCount,
            Items = dataQuery.Select(record => new DepositAccumulateRouletteGameRecordViewModel
            {
                RecordId = record.RouletteId,
                PlayerCode = record.PlayerCode,
                UserName = playerUserNames[record.PlayerCode],
                PromotionId = record.PromotionId,
                PromotionName = promotionNames[record.PromotionId],
                CreatedOn = record.CreatedOn
            }).ToArray()
        };
    }
}
