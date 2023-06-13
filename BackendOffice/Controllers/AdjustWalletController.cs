#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/adjust-wallet")]
[ApiController]
public class AdjustWalletController : ControllerBase
{
    [HttpGet("summary", Name = "QueryAdjustSummaries")]
    public IAsyncEnumerable<AdjustSummaryListItemViewModel> QueryAdjustSummariesAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<AdjustSummariesQuery, AdjustSummaryListItem> queryService,
        [FromQuery] AdjustSummaryQueryViewModel queryViewModel)
        => brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
            .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
            .AsTask()
            .ToAsyncEnumerable()
            .SelectMany(dict => queryService
                .QueryAsync(
                    new((queryViewModel.From, queryViewModel.To)),
                    HttpContext.RequestAborted)
                .Select(record => (Record: record, Info: dict[record.BrandId]))
                .Select(tup => new AdjustSummaryListItemViewModel
                {
                    BrandId = tup.Record.BrandId,
                    Brand = tup.Info.BrandCode,
                    Licensee = tup.Info.LicenseeCode,
                    Agent = tup.Record.Agent,
                    Company = tup.Record.Company,
                    System = tup.Record.System,
                    Transfer = tup.Record.Transfer,
                    Bonus = tup.Record.Bonus,
                    AgentOther = tup.Record.AgentOther
                }))
            .OrderBy(model => model.Licensee)
            .ThenBy(model => model.Brand);

    [HttpGet("agent/{brandId}", Name = "QueryAgentAdjustRecords")]
    public IAsyncEnumerable<AdjustRecordListItemViewModel> QueryAgentAdjustRecordsAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<AgentAdjustRecordsQuery, AdjustRecordListItem> queryService,
        [FromServices] IAgentClient agentClient,
        Guid brandId,
        [FromQuery] AdjustRecordQueryViewModel queryViewModel,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
        => AsyncEnumerable.Zip(
            brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
                .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
                .AsTask()
                .ToAsyncEnumerable(),
            string.IsNullOrWhiteSpace(queryViewModel.UserName)
                ? AsyncEnumerableEx.Return(default(Guid?))
                : agentClient.GetAgentInfoByUsernameAsync(
                    new(brandId, queryViewModel.UserName),
                    HttpContext.RequestAborted)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .Select(info => info?.Id))
            .SelectMany(requestTuple => queryService
                .QueryAsync(new AgentAdjustRecordsQuery(
                    brandId,
                    (queryViewModel.From, queryViewModel.To),
                    queryViewModel.SearchTypes,
                    requestTuple.Second,
                    queryViewModel.Operator,
                    (page - 1) * rows), HttpContext.RequestAborted)
                .Select(item => (Record: item, Info: requestTuple.First[item.BrandId])))
            .Take(rows)
            .Buffer(rows)
            .SelectMany(items => items.ToAsyncEnumerable()
                .Select(tup => agentClient.GetAgentInfoAsync(
                    new(Guid.Parse(tup.Record.UserId)),
                    HttpContext.RequestAborted)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .Select(agentInfo => (tup.Record, BrandInfo: tup.Info, AgentInfo: agentInfo))))
            .Concat()
            .Select(tup => new AdjustRecordListItemViewModel
            {
                Id = tup.Record.Id,
                BrandId = tup.Record.BrandId,
                AdjustType = tup.Record.AdjustType,
                Amount = tup.Record.Amount,
                BrandCode = tup.BrandInfo.BrandCode,
                BucketAmount = tup.Record.BucketAmount,
                CreatedOn = tup.Record.CreatedOn,
                Operator = tup.Record.Operator,
                UserName = tup.AgentInfo?.Username ?? string.Empty,
                UserType = tup.Record.UserType,
                Remark = tup.Record.Remark
            })
            .Take(rows);

    [HttpGet("player/{brandId}", Name = "QueryPlayerAdjustRecords")]
    public IAsyncEnumerable<AdjustRecordListItemViewModel> QueryPlayerAdjustRecordsAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<PlayerAdjustRecordsQuery, AdjustRecordListItem> queryService,
        [FromServices] IPlayerServiceClient playerServiceClient,
        Guid brandId,
        [FromQuery] AdjustRecordQueryViewModel queryViewModel,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
        => AsyncEnumerable.Zip(
            brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
                .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
                .AsTask()
                .ToAsyncEnumerable(),
            string.IsNullOrEmpty(queryViewModel.UserName)
                ? AsyncEnumerableEx.Return(default(string))
                : playerServiceClient.GetPlayerInfoByUserNameAsync(
                    brandId,
                    queryViewModel.UserName,
                    HttpContext.RequestAborted)
                    .AsTask()
                    .ToAsyncEnumerable()
                    .Select(info => info?.PlayerCode))
            .SelectMany(requestTuple => queryService
                .QueryAsync(new PlayerAdjustRecordsQuery(
                    brandId,
                    (queryViewModel.From, queryViewModel.To),
                    queryViewModel.SearchTypes,
                    requestTuple.Second,
                    queryViewModel.Operator,
                    (page - 1) * rows), HttpContext.RequestAborted)
                .Select(item => (Record: item, Info: requestTuple.First[item.BrandId])))
            .Take(rows)
            .Buffer(rows)
            .SelectMany(items => playerServiceClient.GetPlayerCodesByUserNamesAsync(
                brandId,
                items.Select(tup => tup.Record.UserId),
                HttpContext.RequestAborted)
                .ToArrayAsync(HttpContext.RequestAborted)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(playerCodes => playerServiceClient.QueryPlayerInfoListAsync(
                    brandId,
                    playerCodes,
                    HttpContext.RequestAborted)
                    .ToDictionaryAsync(playerInfo => playerInfo.PlayerCode)
                    .AsTask()
                    .ToAsyncEnumerable())
                .SelectMany(dict => items.ToAsyncEnumerable()
                    .Select(item => (item.Record, item.Info, PlayerMap: dict)))
                .Select(tup => new AdjustRecordListItemViewModel
                {
                    Id = tup.Record.Id,
                    BrandId = tup.Record.BrandId,
                    AdjustType = tup.Record.AdjustType,
                    Amount = tup.Record.Amount,
                    BrandCode = tup.Info.BrandCode,
                    BucketAmount = tup.Record.BucketAmount,
                    CreatedOn = tup.Record.CreatedOn,
                    Operator = tup.Record.Operator,
                    UserName = tup.PlayerMap[tup.Record.UserId].UserName,
                    UserType = tup.Record.UserType,
                    Remark = tup.Record.Remark
                }))
            .Take(rows);
}
