using System;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

public record BrandGameCreditProfitQuery(
    Guid BrandId,
    DateOnly Month);
