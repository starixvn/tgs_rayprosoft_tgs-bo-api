#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Tgs.Agent.Client;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/adjust-wallet")]
[ApiController]
public class AdjustWalletController : ControllerBase
{
    [HttpGet("agent/{brandId}:export", Name = "ExportAgentAdjustRecordsExcel")]
    public async Task ExportAgentAdjustRecordsExcelAsync(
        [FromServices] ICommandService<ExportAgentAdjustRecordsCommand> exportService,
        Guid brandId,
        [FromQuery] AdjustSummaryQueryViewModel queryViewModel)
    {
        var responseFeature = HttpContext.Features.Get<IHttpResponseFeature>();
        var responseBodyFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();

        if (responseFeature is not null && responseBodyFeature is not null)
        {
            responseFeature.Headers.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            responseFeature.Headers.ContentDisposition = "attachment; filename=AgentAdjustRecords.xlsx";

            var command = new ExportAgentAdjustRecordsCommand(
                brandId,
                (queryViewModel.From, queryViewModel.To),
                responseBodyFeature.Stream);

            await exportService.ExecuteAsync(
                command,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }

    [HttpGet("player/{brandId}:export", Name = "ExportPlayerAdjustRecordsExcel")]
    public async Task ExportPlayerAdjustRecordsExcelAsync(
        [FromServices] ICommandService<ExportPlayerAdjustRecordsCommand> exportService,
        Guid brandId,
        [FromQuery] AdjustSummaryQueryViewModel queryViewModel)
    {
        var responseFeature = HttpContext.Features.Get<IHttpResponseFeature>();
        var responseBodyFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();

        if (responseFeature is not null && responseBodyFeature is not null)
        {
            responseFeature.Headers.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            responseFeature.Headers.ContentDisposition = "attachment; filename=PlayerAdjustRecords.xlsx";

            var command = new ExportPlayerAdjustRecordsCommand(
                brandId,
                (queryViewModel.From, queryViewModel.To),
                responseBodyFeature.Stream);

            await exportService.ExecuteAsync(
                command,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }

    [HttpGet("summary/{resellerId}", Name = "QueryAdjustSummaries")]
    public IAsyncEnumerable<AdjustSummaryListItemViewModel> QueryAdjustSummariesAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<AllBrandCurrenciesQuery, BrandCurrencyInfo> brandCurrenciesQueryService,
        [FromServices] IQueryService<AdjustSummariesQuery, AdjustSummaryListItem> queryService,
        Guid resellerId,
        [FromQuery] AdjustSummaryQueryViewModel queryViewModel)
        => brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
            .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
            .AsTask()
            .ToAsyncEnumerable()
            .Zip(brandCurrenciesQueryService.QueryAsync(
                new AllBrandCurrenciesQuery(),
                HttpContext.RequestAborted)
                .ToDictionaryAsync(info => info.BrandId, info => info.CurrencyCode)
                .AsTask()
                .ToAsyncEnumerable())
            .SelectMany(dictTup => queryService
                .QueryAsync(
                    new((queryViewModel.From, queryViewModel.To)),
                    HttpContext.RequestAborted)
                .Select(record => (
                    Record: record,
                    Info: dictTup.First[record.BrandId],
                    Currency: dictTup.Second[record.BrandId]))
                .Where(tup => tup.Info.ResellerId == resellerId)
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
                    AgentOther = tup.Record.AgentOther,
                    PlayerAdjustAmount = tup.Record.CompanyAmount + tup.Record.AgentAmount
                        + tup.Record.SystemAmount + tup.Record.TransferAmount
                        + tup.Record.BonusAmount,
                    AgentAdjustAmount = tup.Record.AgentOtherAmount,
                    Currency = tup.Currency
                }))
            .OrderBy(model => model.Licensee)
            .ThenBy(model => model.Brand);

    [HttpGet("agent/{brandId}", Name = "QueryAgentAdjustRecords")]
    public async Task<PagedResponse<AdjustRecordListItemViewModel>> QueryAgentAdjustRecordsAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<AgentAdjustRecordsQuery, AdjustRecordListItem> queryService,
        [FromServices] IGetService<AgentAdjustRecordsQuery, int> countService,
        [FromServices] IAgentClient agentClient,
        Guid brandId,
        [FromQuery] AdjustRecordQueryViewModel queryViewModel,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
    {
        var query = new AgentAdjustRecordsQuery(
            brandId,
            (queryViewModel.From, queryViewModel.To),
            queryViewModel.SearchTypes,
            null,
            queryViewModel.Operator,
            (page - 1) * rows);

        if (!string.IsNullOrWhiteSpace(queryViewModel.UserName))
        {
            var agentInfo = await agentClient.GetAgentInfoByUsernameAsync(
               new(brandId, queryViewModel.UserName),
               HttpContext.RequestAborted).ConfigureAwait(false);

            if (agentInfo is null)
                return new PagedResponse<AdjustRecordListItemViewModel>
                {
                    Valid = true,
                    TotalCount = 0,
                    Items = Array.Empty<AdjustRecordListItemViewModel>()
                };

            query = query with
            {
                AgentId = agentInfo.Id
            };
        }

        var totalCount = await countService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new PagedResponse<AdjustRecordListItemViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = await brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
                .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(brandMap => queryService.QueryAsync(query, HttpContext.RequestAborted)
                    .Select(item => (Record: item, Info: brandMap[item.BrandId])))
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
                .Take(rows)
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
    }

    [HttpGet("player/{brandId}", Name = "QueryPlayerAdjustRecords")]
    public async Task<PagedResponse<AdjustRecordListItemViewModel>> QueryPlayerAdjustRecordsAsync(
        [FromServices] IQueryService<BrandRelationQuery, BrandRelationInfo> brandRelationQueryService,
        [FromServices] IQueryService<PlayerAdjustRecordsQuery, AdjustRecordListItem> queryService,
        [FromServices] IGetService<PlayerAdjustRecordsQuery, int> countService,
        [FromServices] IPlayerServiceClient playerServiceClient,
        Guid brandId,
        [FromQuery] AdjustRecordQueryViewModel queryViewModel,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "rows")] int rows = 10)
    {
        var query = new PlayerAdjustRecordsQuery(
            brandId,
            (queryViewModel.From, queryViewModel.To),
            queryViewModel.SearchTypes,
            null,
            queryViewModel.Operator,
            (page - 1) * rows);

        if (!string.IsNullOrEmpty(queryViewModel.UserName))
        {
            var playerInfo = await playerServiceClient.GetPlayerInfoByUserNameAsync(
                brandId,
                queryViewModel.UserName,
                HttpContext.RequestAborted).ConfigureAwait(false);

            if (playerInfo is null)
                return new PagedResponse<AdjustRecordListItemViewModel>
                {
                    Valid = true,
                    TotalCount = 0,
                    Items = Array.Empty<AdjustRecordListItemViewModel>()
                };

            query = query with
            {
                PlayerCode = playerInfo.PlayerCode
            };
        }

        var totalCount = await countService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new PagedResponse<AdjustRecordListItemViewModel>
        {
            Valid = true,
            TotalCount = totalCount,
            Items = await brandRelationQueryService.QueryAsync(new(), HttpContext.RequestAborted)
                .ToDictionaryAsync(info => info.BrandId, HttpContext.RequestAborted)
                .AsTask()
                .ToAsyncEnumerable()
                .SelectMany(brandMap => queryService.QueryAsync(query, HttpContext.RequestAborted)
                    .Select(item => (Record: item, Info: brandMap[item.BrandId])))
                .Take(rows)
                .Buffer(rows)
                .SelectMany(items => playerServiceClient.QueryPlayerInfoListAsync(
                    brandId,
                    items.Select(item => item.Record.UserId),
                    HttpContext.RequestAborted)
                    .ToDictionaryAsync(playerInfo => playerInfo.PlayerCode)
                    .AsTask()
                    .ToAsyncEnumerable()
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
                .Take(rows)
                .ToArrayAsync(HttpContext.RequestAborted)
                .ConfigureAwait(false)
        };
    }
}
