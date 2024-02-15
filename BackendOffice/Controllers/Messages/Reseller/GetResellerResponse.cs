
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class GetResellerResponse : MessageResponse
{
    public ResellerModel Reseller { get; set; }
}
