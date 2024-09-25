using System;

namespace TGS.Services.BackOffice.ApplicationServices.Menus;

public record SetRoleMenusCommand(string RoleId, string Menus);