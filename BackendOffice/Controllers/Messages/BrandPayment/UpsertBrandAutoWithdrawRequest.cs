using System;
using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class UpsertBrandAutoWithdrawRequest
{
    public decimal AmountUpperLimit { get; set; }

    public IEnumerable<Guid> BrandPaymentIds { get; set; }

    public bool EnableAutoWithdraw { get; set; }
}
