using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;

public class PaymentMethodsRequest : PageableRequest
{
    public string Code { get; internal set; }
}