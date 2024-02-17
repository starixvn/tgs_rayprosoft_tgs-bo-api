using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class GetLicenseeResponse : MessageResponse
{
    public LicenseeModel Licensee { get; set; }
}
