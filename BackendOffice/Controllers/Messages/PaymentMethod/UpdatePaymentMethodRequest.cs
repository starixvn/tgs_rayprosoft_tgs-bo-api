using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;

public class UpdatePaymentMethodRequest
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public Guid PaymentId { get; set; }
}