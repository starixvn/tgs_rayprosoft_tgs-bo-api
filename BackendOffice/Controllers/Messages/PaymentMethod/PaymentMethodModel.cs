using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;

public class PaymentMethodModel
{
    public string BankType { get; set; }

    public string Code { get; set; }

    public Guid Id { get; set; }
}
