using System;

namespace TGS.Services.BackOffice.Application.Queries.Withdraw;

public record GetWithdrawFormDetailQuery(
    Guid FormId,
    Guid BrandId);
