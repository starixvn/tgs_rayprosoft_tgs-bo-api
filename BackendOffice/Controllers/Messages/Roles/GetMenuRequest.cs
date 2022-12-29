using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class GetMenuRequest
{
    public Guid? RoleId { get; set; }

    public RoleType RoleType { get; set; }
}
