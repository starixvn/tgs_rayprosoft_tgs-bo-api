using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetDesignatedUsersResponse : MessageResponse
{
    public string[] Players { get; set; }
}
