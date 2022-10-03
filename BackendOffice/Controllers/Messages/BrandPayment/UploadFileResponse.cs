using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class UploadFileResponse : MessageResponse
{
    public string[] Players { get; set; }
    public string[] NotExistsPlayers { get; set; }
}