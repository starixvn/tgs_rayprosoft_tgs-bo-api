using System;

namespace TGS.Services.BackOffice.Application.Queries.Deposit;

public record GetDepositFormDetailQuery(
    Guid FormId,
    Guid BrandId);
