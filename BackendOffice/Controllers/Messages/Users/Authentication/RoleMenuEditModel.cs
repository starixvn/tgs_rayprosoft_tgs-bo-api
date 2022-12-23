using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;

public class RoleMenuEditModel : BaseRoleMenuEditModel
{
    public RoleSubMenuEditModel[] SubMenus { get; set; }
}

public class RoleSubMenuEditModel : BaseRoleMenuEditModel
{
    //Called SubMenus not Permissions is limited from angular tree framework
    public PermissionEditModel[] SubMenus { get; set; }
}

public class PermissionEditModel
{
    public string Code { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDisplay { get; set; }

    public string Name { get; set; }

    public Guid PermissionId { get; set; }
}
