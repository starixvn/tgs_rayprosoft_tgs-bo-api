using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetDesignatedUsersResponse : MessageResponse
{
    public string[] Players { get; set; }
}
