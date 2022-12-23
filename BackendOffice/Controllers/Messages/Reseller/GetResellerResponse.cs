using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class GetResellerResponse : MessageResponse
{
    public ResellerModel Reseller { get; set; }
}
