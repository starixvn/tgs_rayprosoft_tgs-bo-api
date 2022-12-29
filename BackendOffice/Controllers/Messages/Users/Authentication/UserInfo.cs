using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;

public class UserInfo
{
    public Guid? BrandId { get; set; }

    public bool IsRoot { get; set; }

    public Guid? LicenseeId { get; set; }

    public MenuSetting[] Menus { get; set; }

    public Guid? ResellerId { get; set; }

    public string Role { get; set; }

    public Guid RoleId { get; set; }

    public RoleType RoleType { get; set; }

    public string TimeZone { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; }
}
