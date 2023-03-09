#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.BatchAdjust;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/offline")]
[ApiController]
public class OfflineController : BaseContextController
{
    [HttpHead("{batchId}", Name = "CheckOfflineBackgroundProcessIsCompleted")]
    public async Task CheckBackgroundProcessIsCompletedAsync(
        [FromServices] IGetService<CheckBatchCompletedQuery, bool?> checkService,
        Guid batchId)
    {
        var query = new CheckBatchCompletedQuery(batchId);

        var result = await checkService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var responseFeature = HttpContext.Features.Get<IHttpResponseFeature>()!;

        if (result == true)
            responseFeature.StatusCode = StatusCodes.Status200OK;

        if (result == false)
            responseFeature.StatusCode = StatusCodes.Status202Accepted;

        if (result is null)
            Response.StatusCode = StatusCodes.Status204NoContent;
    }

    [HttpGet("template", Name = "DownloadFormTemplate")]
    public async ValueTask<FileStreamResult> ExportFormTemplateAsync(
        [FromServices] IGetService<ExportOfflinePromotionFormTemplateCommand, Stream> exportService)
    {
        var stream = await exportService
            .GetAsync(new ExportOfflinePromotionFormTemplateCommand(), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpPost("{brandId}/{clientToken?}", Name = "UploadOfflineUserListExcel")]
    public async ValueTask<ResponseViewModel<Guid?>> UploadOfflineUserListExcelAsync(
        [FromServices] IQueryService<ReadOfflinePromotionUserListQuery, string> readService,
        [FromServices] IGetService<SaveOfflineTempUserBatchCommand, Guid> saveService,
        [FromServices] IMessageSender messageSender,
        Guid brandId,
        string? cleintToken,
        [FromForm(Name = "file")] IFormFile excelFile)
    {
        await using var fs = excelFile.OpenReadStream();

        var userNames = await readService
            .QueryAsync(new ReadOfflinePromotionUserListQuery(fs), HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (userNames.Length > 20000)
        {
            return new ResponseViewModel<Guid?>
            {
                Valid = false,
                Message = "Upload data is exceed 20,000"
            };
        }

        var batchId = await saveService
            .GetAsync(new(
                brandId,
                cleintToken,
                userNames,
                Guid.Parse(UserId)), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var request = new ProcessOfflineTempUserBatchRequest
        {
            BatchId = batchId
        };

        await messageSender.PublishAsync(
            "tgs.bo.background.offline-promotion",
            request.ToByteArray(),
            HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new ResponseViewModel<Guid?>
        {
            Valid = true,
            Data = batchId
        };
    }
}
