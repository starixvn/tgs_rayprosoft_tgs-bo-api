using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.OfflineLoginReward;
using Tgs.MessageQueue;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using Tgs.BrandService.Protos;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/offline-login-reward")]
[ApiController]
public class OfflineLoginRewardController : BaseContextController
{
    [HttpGet("template", Name = "ExportOfflineLoginRewardExcelFormTemplate")]
    public async ValueTask<FileStreamResult> ExportFormTemplateAsync(
        [FromServices] IGetService<ExportOfflineLoginRewardFormTemplateCommand, Stream> exportService)
    {
        var stream = await exportService
            .GetAsync(new ExportOfflineLoginRewardFormTemplateCommand(), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpPost("{brandId}", Name = "ImportOfflineLoginRewardBatch")]
    public async ValueTask<UploadOfflineLoginRewardUserListResponse> ImportOfflineLoginRewardBatchAsync(
        [FromServices] IQueryService<ReadOfflineLoginRewardUserListQuery, string> readService,
        [FromServices] IGetService<SaveOfflineLoginRewardTempUserBatchCommand, Guid> saveService,
        [FromServices] IMessageSender messageSender,
        Guid brandId,
        [FromForm(Name = "file")] IFormFile file)
    {
        await using var fs = file.OpenReadStream();

        var userNames = await readService
            .QueryAsync(new(fs), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (userNames.Length > 20000)
        {
            return new UploadOfflineLoginRewardUserListResponse
            {
                Valid =  false,
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
}