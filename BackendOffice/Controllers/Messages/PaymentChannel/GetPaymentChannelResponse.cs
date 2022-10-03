using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PaymentChannel;

public class GetPaymentChannelResponse : MessageResponse
{
    public PaymentChannelModel PaymentChannel { get; set; }
}