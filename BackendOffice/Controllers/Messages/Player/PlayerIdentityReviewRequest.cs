using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Player;

public class PlayerIdentityReviewRequest
{
    public Guid BrandId { get; init; }

    public string IdNumber { get; init; }

    public string RealName { get; init; }

    public string Remark { get; init; }

    public PlayerIdentityReviewResponseState ReviewResponse { get; init; }
}
