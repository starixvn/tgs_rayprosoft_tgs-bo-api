using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record UserInfo(
    Guid Id,
    string Username,
    string Name,
    Guid RoleId,
    Guid? ResellerId,
    Guid? LicenseeId,
    Guid? BrandId,
    Guid DepartmentId,
    string RoleName,
    string DepartmentName,
    string LastLoginIp,
    DateTimeOffset? LastLoginTime,
    string Status,
    DateTimeOffset CreatedOn);
