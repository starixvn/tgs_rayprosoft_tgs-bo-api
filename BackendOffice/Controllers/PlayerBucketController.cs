#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Adjust;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

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
        [FromServices] IGetService<GetPlayerBucketAmountQuery, ReadBucketResult> getAmountService,
        [FromServices] ICommandService<ClearPlayerBucketsCommand> clearService,
        string playerCode)
    {
        var result = await getAmountService.GetAsync(
            new(playerCode),
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (result.Amount > 0 && result.AnchorBucketId is not null)
        {
            var command = new ClearPlayerBucketsCommand(playerCode, result.AnchorBucketId.Value);

            await clearService.ExecuteAsync(
                command,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }

    [HttpGet("amount/{playerCode}", Name = "GetPlayerBucketAmount")]
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
                CreatedOn = evt.CreatedOn,
                Remark = evt.Remark
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
                CleaningBucketAmount = form.CleaningBucketAmount,
                CreatedOn = form.CreatedOn,
                DepositFormId = form.DepositFormId,
                ReviewBalance = form.CurrentBalance,
                ReviewBucketAmount = form.CurrentBucketAmount,
                Status = form.Status.ToString()
            });
    }
}
