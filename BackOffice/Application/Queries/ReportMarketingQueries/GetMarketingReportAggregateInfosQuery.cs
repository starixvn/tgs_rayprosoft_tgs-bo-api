using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

public record GetMarketingReportAggregateInfosQuery(
    Guid BrandId,
    DateOnly StartDate,
    DateOnly EndDate,
    PageableRequest Paging);
