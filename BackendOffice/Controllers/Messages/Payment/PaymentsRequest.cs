using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Payment;

public class PaymentsRequest : PageableRequest
{
    public string Code { get; set; }
}
