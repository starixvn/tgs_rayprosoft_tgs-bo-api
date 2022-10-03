using System;

#nullable enable

namespace TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;

public record CreateWithdrawRequest
{
    public Guid BrandId { get; init; }
    public string PlayerCode { get; init; } = default!;
    public decimal Amount { get; init; }

    public Guid BankInfoId { get; init; }

    public string? Note { get; init; }
}
