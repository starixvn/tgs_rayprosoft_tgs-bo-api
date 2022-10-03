using System;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

public record PaidWithdrawFormCommand(
    Guid BrandId,
    Guid FormId,
    string PaidUserId,
    string PaidUserName);
