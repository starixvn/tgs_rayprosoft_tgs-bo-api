using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/offline-login-reward")]
[ApiController]
public class OfflineLoginRewardController : BaseContextController
{
    [HttpHead("{batchId}", Name = "CheckOfflineLoginRewardBackgroundProcessIsCompleted")]
    public async Task CheckBackgroundProcessIsCompletedAsync(
        [FromServices] IGetService<CheckOfflineLoginRewardBackgroundProcessCompletedQuery, bool?> checkService,
        Guid batchId)
    {
        var query = new CheckOfflineLoginRewardBackgroundProcessCompletedQuery(batchId);

        var result = await checkService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var responseFeature = HttpContext.Features.Get<IHttpResponseFeature>();

        if (result == true)
            responseFeature.StatusCode = StatusCodes.Status200OK;

        if (result == false)
            responseFeature.StatusCode = StatusCodes.Status202Accepted;

        if (result is null)
            Response.StatusCode = StatusCodes.Status204NoContent;
    }

    [HttpPut("{brandId}:create", Name = "CreateOfflineLoginRewardPromotion")]
    public async ValueTask<MessageResponse> CreatePromotionAsync(
        [FromServices] IGetService<GetProcessedUsersQuery, OfflineUsersResult> queryService,
        [FromServices] IPromotionServiceClient promotionServiceClient,
        Guid brandId,
        [FromBody] CreateOfflineLoginRewardPromotionViewModel createViewModel)
    {
        var query = new GetProcessedUsersQuery(createViewModel.BatchId);

        var result = await queryService.GetAsync(
            query,
            HttpContext.RequestAborted).ConfigureAwait(false);

        if (result is null || result.Matchs == 0)
            return new MessageResponse
            {
                Valid = false,
                Message = $"Can't find users of batch id '{createViewModel.BatchId}'"
            };

        var from = new DateTimeOffset(createViewModel.From.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var to = new DateTimeOffset(createViewModel.To.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));

        var createInfo = new CreatePromotionMainInfo
        {
            Name = createViewModel.Name,
            EventDuring = (from.UtcDateTime, to.UtcDateTime),
            PromotionType = "OfflineLoginReward",
            IsAutoSent = true,
            PromotionLimitAmount = null
        };

        var promotionId = await promotionServiceClient.CreatePromotionAsync(
            brandId,
            createInfo,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        var rule = JsonSerializer.SerializeToDocument(new
        {
            createViewModel.BonusAmount,
            createViewModel.TurnoverMultiple,
            PlayerCodes = result.Players
                .Where(player => player.PlayerCode is not null)
                .Select(player => player.PlayerCode)
        }, PromotionUtility.SerializerOptions);

        await promotionServiceClient.SetPromotionRuleAsync(
            promotionId,
            rule,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        await promotionServiceClient.SetPromotionStatusAsync(
            promotionId,
            PromotionStatus.Enabled,
            Username,
            HttpContext.RequestAborted).ConfigureAwait(false);

        return new MessageResponse
        {
            Valid = true
        };
    }
}
