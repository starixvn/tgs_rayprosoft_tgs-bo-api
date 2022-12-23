using System;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public record PlatformCreditPaymentModel
{
    public decimal Amount { get; init; }
    public decimal Balance { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}
