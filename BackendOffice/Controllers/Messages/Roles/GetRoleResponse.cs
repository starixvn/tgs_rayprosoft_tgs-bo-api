using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class GetRoleResponse : MessageResponse
{
    public RoleModel Role { get; set; }
}
