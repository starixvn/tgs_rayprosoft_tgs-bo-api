using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PaymentChannel;

public class UpdatePaymentChannelRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public Guid PaymentId { get; set; }
    public Guid PaymentMethodId { get; set; }
}