using System;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Services.BackOffice.Domain;

public record PlayerIdentityInfo(
    Guid Id,
    string RealName,
    string IdNumber,
    string Data,
    DateTimeOffset CreatedOn,
    PlayerIdentityReviewState Status,
    string Remark,
    DateTimeOffset? ReviewedOn,
    string ReviewedUserName);
