using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages;

public class UploadResponse : MessageResponse
{
    public string Location { get; set; }
}
