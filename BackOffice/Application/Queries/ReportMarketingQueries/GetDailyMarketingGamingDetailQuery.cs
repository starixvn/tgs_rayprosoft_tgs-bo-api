using System;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;
public record GetDailyMarketingGamingDetailQuery(
    Guid BrandId,
    DateOnly QueryDate);
