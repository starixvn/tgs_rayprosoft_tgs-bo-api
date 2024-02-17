using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class GetRoleResponse : MessageResponse
{
    public RoleModel Role { get; set; }
}
