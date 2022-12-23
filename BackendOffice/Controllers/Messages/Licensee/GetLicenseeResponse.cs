using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class GetLicenseeResponse : MessageResponse
{
    public LicenseeModel Licensee { get; set; }
}
