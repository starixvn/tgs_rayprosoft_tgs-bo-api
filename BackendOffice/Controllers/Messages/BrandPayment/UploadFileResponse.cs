using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class UploadFileResponse : MessageResponse
{
    public string[] NotExistsPlayers { get; set; }

    public string[] Players { get; set; }
}
