using Microsoft.AspNetCore.Http;

namespace TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;

public class ImageUploadRequest
{
    public IFormFile FormFile { get; set; }
}
