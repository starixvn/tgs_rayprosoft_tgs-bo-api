using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class GetResellersRequest : PageableRequest
{
    public string Keyword { get; set; }
}