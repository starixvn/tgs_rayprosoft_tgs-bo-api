#nullable enable

using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Withdraw;

public record WithdrawReviewRequest
{
    public required Guid BrandId { get; init; }

    public Guid? BrandPaymentId { get; init; }

    public string? Remark { get; init; }

    public string? PlayerRemark { get; init; }

    public required ReviewResponseState ReviewResponse { get; init; }
}
