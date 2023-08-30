#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Adjust;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/player/bucket")]
[ApiController]
public class PlayerBucketController : BaseContextController
{
    [HttpPut("{playerCode}:adjust-increase", Name = "AdjustIncreasePlayerBucketAmount")]
    public ValueTask AdjustIncreasePlayerBucketAmountAsync(
        [FromServices] ICommandService<AdjustPlayerBucketCommand> adjustService,
        string playerCode,
        [FromBody] AdjustPlayerBucketViewModel viewModel)
    {
        var command = new AdjustPlayerBucketCommand(playerCode, viewModel.Amount, viewModel.Remark);

        return adjustService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpDelete("{playerCode}", Name = "ClearPlayerBuckets2")]
    public async Task ClearPlayerBucketsAsync(
        [FromServices] IGetService<GetPlayerInfoByPlayerCodeQuery, PlayerInfo?> getPlayerInfoService,
        [FromServices] IQueryService<GetBrandBucketTypesQuery, string> getBrandBucketTypesService,
        [FromServices] ICommandService<ClearPlayerTypeBucketsCommand> clearService,
        string playerCode)
    {
        var player = await getPlayerInfoService.GetAsync(
            new(playerCode),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (player is null)
            return;

        var brandBucketTypes = await getBrandBucketTypesService
            .QueryAsync(new(player.BrandId), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var command = new ClearPlayerTypeBucketsCommand(player.PlayerCode, brandBucketTypes);

        await clearService.ExecuteAsync(
            command,
            HttpContext.RequestAborted).ConfigureAwait(false);
    }

    [HttpGet("type/amount/{playerCode}", Name = "GetAllTypeBucketAmounts")]
    public IAsyncEnumerable<TypeBucketResultViewModel> GetAllTypeBucketAmountsAsync(
        [FromServices] IGetService<GetPlayerInfoByPlayerCodeQuery, PlayerInfo?> getPlayerInfoService,
        [FromServices] IQueryService<GetBrandBucketTypesQuery, string> getBrandBucketTypesService,
        [FromServices] IQueryService<GetPlayerAllTypeBucketAmountsQuery, TypeBucketResult> getAllTypeBucketAmountsService,
        string playerCode)
        => getPlayerInfoService.GetAsync(
            new(playerCode),
            HttpContext.RequestAborted)
            .AsTask()
            .ToAsyncEnumerable()
            .Where(player => player is not null)
            .Select(player => new GetPlayerAllTypeBucketAmountsQuery(
                player!.PlayerCode,
                getBrandBucketTypesService
                    .QueryAsync(new(player!.BrandId), HttpContext.RequestAborted)
                    .ToEnumerable()))
            .SelectMany(query => getAllTypeBucketAmountsService.QueryAsync(
                query,
                HttpContext.RequestAborted))
            .Select(result => new TypeBucketResultViewModel
            {
                BucketType = result.BucketType,
                Amount = result.Result.Amount
            });

    [HttpGet("amount/{playerCode}", Name = "GetPlayerBucketAmount")]
    public async Task<decimal> GetPlayerBucketAmountAsync(
        [FromServices] IGetService<GetPlayerInfoByPlayerCodeQuery, PlayerInfo?> getPlayerInfoService,
        [FromServices] IQueryService<GetBrandBucketTypesQuery, string> getBrandBucketTypesService,
        [FromServices] IQueryService<GetPlayerAllTypeBucketAmountsQuery, TypeBucketResult> getAllTypeBucketAmountsService,
        string playerCode)
    {
        var player = await getPlayerInfoService.GetAsync(
            new(playerCode),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (player is null)
            return 0M;

        var brandBucketTypes = await getBrandBucketTypesService
            .QueryAsync(new(player.BrandId), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var query = new GetPlayerAllTypeBucketAmountsQuery(playerCode, brandBucketTypes);

        var amount = await getAllTypeBucketAmountsService
            .QueryAsync(query, HttpContext.RequestAborted)
            .Select(result => result.Result.Amount)
            .MaxAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return amount;
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
                CreatedOn = evt.CreatedOn,
                Remark = evt.Remark
            });
    }

    [HttpGet("{playerCode}/{bucketType}", Name = "GetPlayerTypeBuckets")]
    [HttpGet("{playerCode}", Name = "GetPlayerBuckets2")]
    public IAsyncEnumerable<PlayerBucketViewModel> GetPlayerBucketsAsync(
        [FromServices] IQueryService<GetPlayerBucketsQuery, BucketInfo> queryService,
        string playerCode,
        string? bucketType)
    {
        var query = new GetPlayerBucketsQuery(playerCode, bucketType);

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Select(info => new PlayerBucketViewModel
            {
                Id = info.Id,
                BucketType = info.Type,
                PlayerCode = info.PlayerCode,
                WageringRequirement = info.Amount,
                UncompleteWagering = info.IsFull ? 0M : info.Amount - info.FillInAmount,
                CreatedOn = info.CreatedOn
            });
    }

    [HttpGet("clean-form/{playerCode}", Name = "QueryCleanBucketForms")]
    [HttpGet("clean-form/{playerCode}/{flowStatus}", Name = "QueryCleanBucketFormsByFlowStatus")]
    public IAsyncEnumerable<CleanBucketFormViewModel> QueryCleanBucketFormsAsync(
        [FromServices] IQueryService<GetCleanBucketFormListQuery, CleanBucketForm> queryService,
        string playerCode,
        string? flowStatus)
    {
        var query = new GetCleanBucketFormListQuery(
            playerCode,
            string.IsNullOrWhiteSpace(flowStatus)
                ? null
                : Enum.Parse<CleanFlowStatus>(flowStatus));

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Select(form => new CleanBucketFormViewModel
            {
                Id = form.Id,
                PlayerCode = form.PlayerCode,
                CleaningBalance = form.CleaningBalance,
                CleaningBucketAmount = form.CleaningBucketAmounts
                    ?.FirstOrDefault(result => result.BucketType is null)
                    ?.Result
                    .Amount,
                CreatedOn = form.CreatedOn,
                DepositFormId = form.DepositFormId,
                ReviewBalance = form.CurrentBalance,
                ReviewBucketAmount = form.CurrentBucketAmounts
                    .First()
                    .Result
                    .Amount,
                Status = form.Status.ToString()
            });
    }
}
