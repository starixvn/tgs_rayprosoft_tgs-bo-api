using System;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayerIdentityAuthenticationListRequest
{
    public DateTimeOffset? BeginCreatedOn { get; init; }

    public DateTimeOffset? BeginReviewedOn { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset? EndCreatedOn { get; init; }

    public DateTimeOffset? EndReviewedOn { get; init; }

    public PlayerIdentityReviewState? Status { get; init; }

    public string? Username { get; init; }
}
