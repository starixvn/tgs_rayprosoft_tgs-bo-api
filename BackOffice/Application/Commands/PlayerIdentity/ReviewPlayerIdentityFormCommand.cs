using System;

namespace TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

public record ReviewPlayerIdentityFormCommand(
    Guid BrandId,
    Guid FormId,
    PlayerIdentityReviewState Status,
    string Remark,
    string ReviewUserId,
    string ReviewUserName,
    string RealName,
    string IdNumber);
