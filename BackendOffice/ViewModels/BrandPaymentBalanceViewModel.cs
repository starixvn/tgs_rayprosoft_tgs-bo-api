using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public class BrandPaymentBalanceViewModel
{
    public Guid BrandPaymentId { get; set; }

    public decimal? Balance { get; set; }
}
