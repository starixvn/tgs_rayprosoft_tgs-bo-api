using System;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

public record BrandPaymentsQuery(Guid BrandId, string PlayerCode);
