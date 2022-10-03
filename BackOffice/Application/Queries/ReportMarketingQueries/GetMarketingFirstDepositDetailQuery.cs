using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

public record GetMarketingFirstDepositDetailQuery(
    Guid BrandId,
    DateOnly Date,
    string AgentUsername,
    PageableRequest Paging);
