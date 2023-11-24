using System;
using Tgs.Paging;

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetUsersQuery(
    Guid? ResellerId,
    Guid? LicenseeId,
    Guid? BrandId,
    string Username,
    string DepartmentName,
    string RoleName,
    Paging<UserInfo> Paging);
