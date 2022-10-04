using System;

namespace TGS.Services.BackOffice.ApplicationServices;
public record PlayerPaymentStatisticsDetailQuery(Guid BrandId, DateOnly QueryDate);
