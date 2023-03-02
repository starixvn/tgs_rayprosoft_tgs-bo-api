using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
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
}
