using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class UpdateRoleRequest
{
    public MenuSetting[] Menus { get; set; }

    public string Name { get; set; }
}
