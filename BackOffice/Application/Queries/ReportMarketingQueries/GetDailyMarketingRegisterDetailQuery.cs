using System;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;
public record GetDailyMarketingRegisterDetailQuery(
    Guid BrandId,
    DateOnly QueryDate);
