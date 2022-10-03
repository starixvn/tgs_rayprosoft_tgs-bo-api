using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class CreateUserRequest
{
    public string Username { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string TimeZone { get; set; }
    public Guid RoleId { get; set; }
    public Guid DepartmentId { get; set; }
    public UserStatus Status { get; set; }
}