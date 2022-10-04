using System;
using Tgs.Paging;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;
using TGS.Services.BackOffice.Domain;

#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetPlayerIdentityAuthenticationFormsQuery(
    Guid BrandId,
    PlayerIdentityReviewState? Status,
    string? Username,
    DateTimeOffset? BeginCreatedOn,
    DateTimeOffset? EndCreatedOn,
    DateTimeOffset? BeginReviewedOn,
    DateTimeOffset? EndReviewedOn,
    Paging<PlayerIdentityAuthenticationInfo> Paging);
