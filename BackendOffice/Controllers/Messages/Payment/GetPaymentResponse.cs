using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Payment;

public class GetPaymentResponse : MessageResponse
{
    public PaymentModel Payment { get; set; }
}