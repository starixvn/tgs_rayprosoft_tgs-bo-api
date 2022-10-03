using System;

namespace TGS.Services.BackOffice.Application.Services;
public record PlayerPaymentStatisticsDetailQuery(Guid BrandId, DateOnly QueryDate);
