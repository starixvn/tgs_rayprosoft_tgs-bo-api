using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class GetResellersRequest : PageableRequest
{
    public string Keyword { get; set; }
}
