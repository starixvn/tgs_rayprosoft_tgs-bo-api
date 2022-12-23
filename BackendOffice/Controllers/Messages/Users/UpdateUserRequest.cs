using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class UpdateUserRequest
{
    public Guid DepartmentId { get; set; }

    public string Name { get; set; }

    public Guid RoleId { get; set; }

    public UserStatus Status { get; set; }
}
