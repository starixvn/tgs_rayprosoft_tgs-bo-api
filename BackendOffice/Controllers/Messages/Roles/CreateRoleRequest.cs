using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class CreateRoleRequest
{
    public Guid TemplateId { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public bool IsRoot { get; set; }
    public Guid? ResellerId { get; set; }
    public Guid? LicenseeId { get; set; }
    public Guid? BrandId { get; set; }
    public RoleType RoleType { get; set; }
    public MenuSetting[] Menus { get; set; }
}