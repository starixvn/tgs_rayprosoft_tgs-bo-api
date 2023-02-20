using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
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
}