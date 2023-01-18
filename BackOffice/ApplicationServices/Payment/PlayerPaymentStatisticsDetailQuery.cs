using System;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;
public record PlayerPaymentStatisticsDetailQuery(Guid BrandId, DateOnly QueryDate);
