using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record DepartmentInfo(
    Guid Id,
    Guid? ResellerId,
    Guid? LicenseeId,
    Guid? BrandId,
    string ResellerName,
    string LicenseeName,
    string BrandName,
    string Name);
