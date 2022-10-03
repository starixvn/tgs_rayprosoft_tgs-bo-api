using System;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Services.BackOffice.Domain;

public record PlayerIdentityAuthenticationInfo(
    string Data,
    string IdNumber,
    string RealName,
    string Remark,
    PlayerIdentityReviewState Status,
    string Username,
    DateTimeOffset CreatedOn,
    Guid PlayerAuthenticationFormId,
    string PlayerCode);
