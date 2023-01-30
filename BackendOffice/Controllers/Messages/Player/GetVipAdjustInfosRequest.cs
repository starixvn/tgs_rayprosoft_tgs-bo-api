using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Player;

public record GetVipAdjustInfosRequest
{
    public DateTimeOffset From { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset To { get; init; }
}
