using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

namespace TGS.Apis.BackendOffice.Controllers.Promotion;

[Route("api/offline")]
[ApiController]
public class OfflineController : ControllerBase
{
    [HttpGet("template", Name = "DownloadFormTemplate")]
    public async ValueTask<FileStreamResult> ExportFormTemplateAsync(
        [FromServices] IGetService<ExportOfflinePromotionFormTemplateCommand, Stream> exportService)
    {
        var stream = await exportService
            .GetAsync(new ExportOfflinePromotionFormTemplateCommand(), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpPost("{brandId}", Name = "UploadOfflineUserListExcel")]
    public async ValueTask<ResponseViewModel<Guid?>> UploadOfflineUserListExcelAsync(
        [FromServices] IQueryService<ReadOfflinePromotionUserListQuery, string> readService,
        [FromServices] IGetService<SaveOfflineTempUserBatchCommand, Guid> saveService,
        [FromServices] IMessageSender messageSender,
        Guid brandId,
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
            .GetAsync(new(brandId, userNames), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        var request = new ProcessOfflineTempUserBatchRequest
        {
            BatchId = batchId
        };

        await messageSender.PublishAsync(
            "tgs.bo.background.offline-promotion.temp_user_batch",
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
