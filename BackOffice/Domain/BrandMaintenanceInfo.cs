#nullable enable

using System;
using Tgs.BrandCertificate;

namespace TGS.Services.BackOffice.Domain;

public record BrandMaintenanceInfo(
    string BrandName,
    string BrandCode,
    string LicenseeName,
    Guid BrandId,
    BrandStatus BrandStatus,
    string? BeginTime = null,
    string? EndTime = null,
    Guid? BrandMaintenanceId = null);

