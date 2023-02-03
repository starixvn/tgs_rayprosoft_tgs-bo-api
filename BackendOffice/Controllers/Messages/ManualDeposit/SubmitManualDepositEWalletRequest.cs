using System;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers.Messages.ManualDeposit;

public record SubmitManualDepositEWalletRequest
{
    public string PlayerCode { get; init; } = default!;

    public Guid BrandId { get; init; }

    public decimal Amount { get; init; }

    public Guid BrandPaymentId { get; init; }

    public Guid? PromotionId { get; init; }

    public string BankType { get; init; } = default!;

    public string? Note { get; init; }

    public string? RealName { get; init; }
    public string? Phone { get; init; }
}
