using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class RoleModel
{
    public Guid Id { get; init; }
    public RoleType RoleType { get; init; }
    public Guid? ResellerId { get; init; }
    public Guid? LicenseeId { get; init; }
    public Guid? BrandId { get; init; }
    public string ResellerName { get; init; }
    public string LicenseeName { get; init; }
    public string BrandName { get; init; }
    public string Name { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }
    public bool IsRoot { get; init; }
    public MenuSetting[] Menus { get; init; }
    public string CreatedByUsername { get; init; }
    public string UpdatedByUsername { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
    public DateTimeOffset UpdatedOn { get; init; }
}