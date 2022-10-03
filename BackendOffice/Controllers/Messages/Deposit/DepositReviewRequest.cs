using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public record DepositReviewRequest
{
    public Guid BrandId { get; init; }

    public string Remark { get; init; }

    public ReviewResponseState ReviewResponse { get; init; }

    public decimal? ActualAmount { get; init; }
}
