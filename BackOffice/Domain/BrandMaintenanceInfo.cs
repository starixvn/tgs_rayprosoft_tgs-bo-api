using System;

namespace TGS.Services.BackOffice.Domain;

public record BrandMaintenanceInfo(
    string BrandName,
    string BrandCode,
    string LicenseeName,
    Guid BrandId,
    TimeOnly? BeginTime = null,
    TimeOnly? EndTime = null,
    Guid? Id = null);
