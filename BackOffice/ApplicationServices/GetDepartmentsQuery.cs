using System;
using Tgs.Paging;

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetDepartmentsQuery(
    Guid? ResellerId,
    Guid? LicenseeId,
    Guid? BrandId,
    string DepartmentName,
    Paging<DepartmentInfo> Paging);
