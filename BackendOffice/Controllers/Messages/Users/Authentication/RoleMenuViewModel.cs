using System.Collections.Generic;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;

public class RoleMenuViewModel : BaseRoleMenuViewModel
{
    public List<RoleSubMenuViewModel> SubMenus { get; set; }
}
