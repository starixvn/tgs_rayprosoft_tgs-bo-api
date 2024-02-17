using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

public record GetMarketingReportAggregateInfosQuery(
    Guid BrandId,
    DateOnly StartDate,
    DateOnly EndDate,
    PageableRequest Paging);
