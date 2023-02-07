using System;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

public record CreateDepositFormCommand(
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    string Subject,
    Guid? PromotionId,
    Func<Guid, string?> CreatePaymentSettings);
