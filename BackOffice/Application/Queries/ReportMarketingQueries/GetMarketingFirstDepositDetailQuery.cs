using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

public record GetMarketingFirstDepositDetailQuery(
    Guid BrandId,
    DateOnly Date,
    string AgentUsername,
    PageableRequest Paging);
