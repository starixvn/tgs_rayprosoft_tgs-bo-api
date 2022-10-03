using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.StationLetter;

public class ImageUploadResponse : MessageResponse
{
    public string Url { get; set; }
}