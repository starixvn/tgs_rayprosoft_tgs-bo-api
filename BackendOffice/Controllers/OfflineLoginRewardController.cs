using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.OfflineLoginReward;
using TGS.Services.BackOffice.ApplicationServices.Promotion;

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

        return new UploadOfflineLoginRewardUserListResponse
        {
            Valid = true,
            BatchId = batchId
        };
    }
}