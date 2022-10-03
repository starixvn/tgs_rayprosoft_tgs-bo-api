using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string DepartmentName { get; set; }
    public string Role { get; set; }
    public string LastLoginIp { get; set; }
    public Guid RoleId { get; set; }
    public Guid? ResellerId { get; set; }
    public Guid? LicenseeId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTimeOffset? LastLoginTime { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string Status { get; set; }
}
