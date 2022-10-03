using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

public record GetMarketingReportAggregateInfosByAgentQuery(
    Guid BrandId,
    string AgentUserName,
    DateOnly StartDate,
    DateOnly EndDate,
    PageableRequest Paging);
