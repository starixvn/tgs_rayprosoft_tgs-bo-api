using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Localization;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocalizationController : ControllerBase
{
    [HttpGet("export/{systemCode}")]
    public async Task<FileContentResult> ExportLocalizationFileAsync(
        [FromServices] IGetService<ExportLocalizationFileCommand, Stream> exportService,
        string systemCode)
    {
        await using var fs = await exportService
            .GetAsync(new ExportLocalizationFileCommand(systemCode))
            .ConfigureAwait(false);

        var content = new byte[fs.Length];

        _ = await fs.ReadAsync(new Memory<byte>(content)).ConfigureAwait(false);

        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [HttpPost("import/{systemCode}")]
    public async Task ImportLocalizationFileAsync(
        [FromServices] ICommandService<ImportLocalizationFileCommand> importService,
        string systemCode,
        [FromForm(Name = "file")] IFormFile file)
    {
        await using var fs = file.OpenReadStream();

        var command = new ImportLocalizationFileCommand(systemCode, fs);

        await importService.ExecuteAsync(command).ConfigureAwait(false);
    }
}
