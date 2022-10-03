using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;

public class PaymentMethodModel
{
    public Guid Id { get; set; }

    public string Code { get; set; }

    public string BankType { get; set; }
}
