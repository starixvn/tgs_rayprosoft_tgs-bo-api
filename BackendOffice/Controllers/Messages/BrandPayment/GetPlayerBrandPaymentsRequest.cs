using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;

public class GetPlayerBrandPaymentsRequest
{
    public Guid BrandId { get; set; }

    public string PlayerCode { get; set; }
}
