using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.Promotion;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.OfflineLoginReward;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using TGS.Services.BackOffice.Domain;
namespace TGS.Apis.BackendOffice.Controllers;

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

    [HttpGet("template", Name = "ExportOfflineLoginRewardExcelFormTemplate")]
    [HttpGet("~/api/offline-deposit-reward/template", Name = "ExportOfflineDepositRewardExcelFormTemplate")]
    public async ValueTask<FileStreamResult> ExportFormTemplateAsync(
        [FromServices] IGetService<ExportOfflinePromotionFormTemplateCommand, Stream> exportService)
    {
        var stream = await exportService
            .GetAsync(new ExportOfflinePromotionFormTemplateCommand(), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpPost("{brandId}", Name = "ImportOfflineLoginRewardBatch")]
    public async ValueTask<UploadOfflineLoginRewardUserListResponse> ImportOfflineLoginRewardBatchAsync(
        [FromServices] IQueryService<ReadOfflinePromotionUserListQuery, string> readService,
        [FromServices] IGetService<SaveOfflineLoginRewardTempUserBatchCommand, Guid> saveService,
        [FromServices] IMessageSender messageSender,
        Guid brandId,
        [FromForm(Name = "file")] IFormFile file)
    {
        await using var fs = file.OpenReadStream();

        var userNames = await readService
            .QueryAsync(new ReadOfflinePromotionUserListQuery(fs), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (userNames.Length > 20000)
        {
            return new UploadOfflineLoginRewardUserListResponse
            {
                Valid = false,
                Message = "Upload data is exceed 20,000"
            };
        }

        var batchId = await saveService
            .GetAsync(new(brandId, userNames), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var request = new ProcessOfflineLoginRewardTempUserBatchRequest
        {
            BatchId = batchId
        };

        await messageSender.PublishAsync(
            "tgs.bo.background.offlineloginreward.temp_user_batch",
            request.ToByteArray(),
            HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new UploadOfflineLoginRewardUserListResponse
        {
            Valid = true,
            BatchId = batchId
        };
    }

    [HttpPost("{brandId}:create", Name = "CreateOfflineLoginRewardPromotion")]
    public async ValueTask<MessageResponse> CreatePromotionAsync(
        [FromServices] IQueryService<GetProcessedUsersQuery, OfflinePlayerItem> queryService,
        [FromServices] IPromotionServiceClient promotionServiceClient,
        Guid brandId,
        [FromBody] CreateOfflineLoginRewardPromotionViewModel createViewModel)
    {
        var query = new GetProcessedUsersQuery(createViewModel.BatchId);

        var players = await queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Where(item => !string.IsNullOrEmpty(item.PlayerCode))
            .Select(item => item.PlayerCode)
            .Distinct()
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (players.Length == 0)
        {
            return new MessageResponse
            {
                Valid = false,
                Message = $"Can't find users of batch id '{createViewModel.BatchId}'"
            };
        }

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
            PlayerCodes = players
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