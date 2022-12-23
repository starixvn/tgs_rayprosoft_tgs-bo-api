using System;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers.Messages.ManualDeposit;

public record SubmitManualDepositBankRequest
{
    public string PlayerCode { get; init; } = default!;

    public Guid BrandId { get; init; }

    public string BankAccount { get; init; } = default!;

    public string? BankName { get; init; }

    public decimal Amount { get; init; }

    public Guid BrandPaymentId { get; init; }

    public Guid? PromotionId { get; init; }

    public string BankType { get; init; } = default!;

    public string? Note { get; init; }
}
