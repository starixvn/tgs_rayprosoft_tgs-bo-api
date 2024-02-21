using System;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;
public record GetBrandPaymentBalanceQuery(
    Guid BrandId,
    Guid BrandPaymentId);
