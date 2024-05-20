using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.FileService.Clients;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Resources;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Utility;
using TGS.Core.Models;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/fs")]
[ApiController]
public class FileController : BaseContextController
{
    /// <summary>
    /// Gets the file.
    /// </summary>
    /// <param name="fileServiceClient">The file service client.</param>
    /// <param name="objectId">The object identifier.</param>
    /// <returns></returns>
    [HttpGet("{objectId}")]
    [ByPassAdminLog]
    public async Task<IActionResult> GetFileAsync(
        [FromServices] IFileServiceClient fileServiceClient,
        string objectId)
    {
        var fileView = await fileServiceClient.FindFileByObjectIdAsync(objectId).ConfigureAwait(false);

        return File(
            await fileServiceClient
                .OpenFileReaderAsync(fileView.FileName)
                .ConfigureAwait(false),
            fileView.ContentType);
    }

    [HttpPost("upload/{prefix}")]
    [ByPassAdminLog]
    public async Task<UploadResponse> UploadFileAsync(
        [FromServices] IFileServiceClient fileServiceClient,
        string prefix,
        [FromForm(Name = "file")] IFormFile file)
    {
        var type = Consts.UploadFile.Types.FirstOrDefault(name => name == prefix);

        if (type.IsNullOrEmpty())
        {
            return CreateInvalidResponse<UploadResponse>(StationLetter.TypeNotFound);
        }

        var contentType = file.ContentType;
        await using var fileStream = file.OpenReadStream();
        var randomFileName =
            $"{StringGenerator.GenerateAlphanumericString(32)}{Path.GetExtension(file.FileName)}";

        var path = $"/{type}/{randomFileName}";

        var fileView = await fileServiceClient.UploadPublicFileAsync(path,
            MediaTypeHeaderValue.Parse(contentType),
            fileStream);
        return new()
        {
            Valid = true,
            Location = fileView.ExternalUrl
        };
    }
}
