using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TGS.Apis.BackendOffice.Controllers.Messages.Roles;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController : BaseContextController
{
    private readonly ITgsQueries<Core.DataServices.Tgs.Models.Brand> _brandQueries;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;
    private readonly ITgsQueries<Core.DataServices.Tgs.Models.Licensee> _licenseeQueries;
    private readonly ITgsQueries<Core.DataServices.Tgs.Models.Reseller> _resellerQueries;
    private readonly ITgsCommands<Role> _roleCommands;
    private readonly ITgsQueries<Role> _roleQueries;
    private readonly ITgsQueries<Setting> _settingQueries;

    public RoleController(
        ITgsQueries<Role> roleQueries,
        ITgsQueries<Setting> settingQueries,
        ITgsCommands<Role> roleCommands,
        ITgsQueries<Core.DataServices.Tgs.Models.Reseller> resellerQueries,
        ITgsQueries<Core.DataServices.Tgs.Models.Licensee> licenseeQueries,
        ITgsQueries<Core.DataServices.Tgs.Models.Brand> brandQueries,
        IJsonSerializationProvider jsonSerializationProvider)
    {
        _roleQueries = roleQueries;
        _roleCommands = roleCommands;
        _settingQueries = settingQueries;
        _resellerQueries = resellerQueries;
        _licenseeQueries = licenseeQueries;
        _brandQueries = brandQueries;
        _jsonSerializationProvider = jsonSerializationProvider;
    }

    [HttpGet]
    [Description("Get roles")]
    public Task<PagedResponse<RoleModel>> GetListAsync([FromQuery] GetRolesRequest request) =>
        _roleQueries.GetQueries()
            .Where(request.IsRoot, x => x.IsRoot == request.IsRoot)
            .Where(request.ResellerId, x => x.ResellerId == request.ResellerId)
            .Where(request.LicenseeId, x => x.LicenseeId == request.LicenseeId)
            .Where(request.BrandId, x => x.BrandId == request.BrandId)
            .Where(ResellerId, x => x.ResellerId == ResellerId)
            .Where(LicenseeId, x => x.LicenseeId == LicenseeId)
            .Where(BrandId, x => x.BrandId == BrandId)
            .Where(request.Name, x => EF.Functions.ILike(x.Name, $"%{request.Name}%"))
            .GroupJoin(_resellerQueries.GetQueries(),
                r => r.ResellerId,
                r => r.Id,
                (role, resellers) => new
                {
                    role,
                    resellers
                })
            .SelectMany(x => x.resellers.DefaultIfEmpty(),
                (x, reseller) => new
                {
                    x.role,
                    reseller
                })
            .GroupJoin(_licenseeQueries.GetQueries(),
                combine => combine.role.LicenseeId,
                l => l.Id,
                (combine, licensees) => new
                {
                    combine.role,
                    combine.reseller,
                    licensees
                })
            .SelectMany(x => x.licensees.DefaultIfEmpty(),
                (x, licensee) => new
                {
                    x.role,
                    x.reseller,
                    licensee
                })
            .GroupJoin(_brandQueries.GetQueries(),
                combine => combine.role.BrandId,
                b => b.Id,
                (combine, brands) => new
                {
                    combine.role,
                    combine.reseller,
                    combine.licensee,
                    brands
                })
            .SelectMany(x => x.brands.DefaultIfEmpty(),
                (x, brand) => new
                {
                    x.role,
                    x.reseller,
                    x.licensee,
                    brand
                })
            .Select(x => new RoleModel
            {
                Id = x.role.Id,
                RoleType = x.role.RoleType,
                ResellerId = x.role.ResellerId,
                LicenseeId = x.role.LicenseeId,
                BrandId = x.role.BrandId,
                ResellerName = x.reseller != null ? x.reseller.Name : null,
                LicenseeName = x.licensee != null ? x.licensee.Name : null,
                BrandName = x.brand != null ? x.brand.Name : null,
                Name = x.role.Name,
                DisplayName = x.role.DisplayName,
                Description = x.role.Description,
                IsRoot = x.role.IsRoot,
                CreatedOn = x.role.CreatedOn,
                UpdatedOn = x.role.UpdatedOn,
                CreatedByUsername = x.role.CreatedByUsername,
                UpdatedByUsername = x.role.UpdatedByUsername
            })
            .CreatePagedResponseAsync(request);

    [HttpGet("{roleId}")]
    [Description("Get a role")]
    public async Task<GetRoleResponse> GetSingleAsync(Guid roleId)
    {
        var role = await _roleQueries.GetSingleAsync(p => p.Id == roleId,
            x => x
                .Where(ResellerId != null, r => r.ResellerId == ResellerId)
                .Where(LicenseeId != null, l => l.LicenseeId == LicenseeId)
                .Where(BrandId != null, l => l.BrandId == BrandId));

        if (role == null)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.NotFound);
        }

        var menuSetting = role.IsRoot switch
        {
            true when role.BrandId != null => (await _roleQueries.GetSingleAsync(x =>
                x.IsRoot && x.LicenseeId == role.LicenseeId && x.BrandId == null)).MenuSetting,
            true when role.LicenseeId != null => (await _roleQueries.GetSingleAsync(x =>
                    x.IsRoot && x.ResellerId == role.ResellerId && x.LicenseeId == null && x.BrandId == null))
                .MenuSetting,
            true when role.ResellerId != null =>
                (await _settingQueries.GetSingleAsync(p => p.Key == "Menus")).Value,
            false when role.BrandId != null => (await _roleQueries.GetSingleAsync(x =>
                x.IsRoot && x.BrandId == role.BrandId)).MenuSetting,
            false when role.LicenseeId != null => (await _roleQueries.GetSingleAsync(x =>
                x.IsRoot && x.LicenseeId == role.LicenseeId && x.BrandId == null)).MenuSetting,
            false when role.ResellerId != null => (await _roleQueries.GetSingleAsync(x =>
                    x.IsRoot && x.ResellerId == role.ResellerId && x.LicenseeId == null && x.BrandId == null))
                .MenuSetting,
            _ => (await _settingQueries.GetSingleAsync(p => p.Key == "Menus")).Value
        };

        var roleModel = new RoleModel
        {
            Id = role.Id,
            RoleType = role.RoleType,
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            IsRoot = role.IsRoot,
            Menus = GetMenuSetting(menuSetting, role.MenuSetting),
            CreatedOn = role.CreatedOn,
            UpdatedOn = role.UpdatedOn,
            CreatedByUsername = role.CreatedByUsername,
            UpdatedByUsername = role.UpdatedByUsername
        };

        return new()
        {
            Valid = true,
            Role = roleModel
        };
    }

    [HttpPut("{roleId}")]
    [Description("Update role")]
    public async Task<MessageResponse> UpdateAsync(
        Guid roleId,
        [FromBody] UpdateRoleRequest request)
    {
        var role = await _roleQueries.GetSingleAsync(p => p.Id == roleId);

        if (role == null)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.NotFound);
        }

        var roleNameExisted = await _roleQueries.GetQueries()
            .Where(role.ResellerId, x => x.ResellerId == role.ResellerId)
            .Where(role.LicenseeId, x => x.LicenseeId == role.LicenseeId)
            .Where(role.BrandId, x => x.BrandId == role.BrandId)
            .AnyAsync(x => x.Id != role.Id && x.Name == request.Name);

        if (roleNameExisted)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.NameExists);
        }

        role.Name = request.Name;
        role.MenuSetting = _jsonSerializationProvider.SerializeToString(request.Menus);
        role.UpdatedByUsername = Username;
        role.UpdatedOn = DateTimeOffset.Now;

        if (!role.IsRoot || role.RoleType == RoleType.Admin)
        {
            await _roleCommands.UpdateAsync(role);

            return CreateValidResponse<MessageResponse>();
        }

        var roles = await _roleQueries.GetQueries()
            .Where(role.RoleType == RoleType.Brand, x => x.BrandId == role.BrandId)
            .Where(role.RoleType == RoleType.Licensee, x => x.LicenseeId == role.LicenseeId)
            .Where(role.RoleType == RoleType.Reseller, x => x.ResellerId == role.ResellerId).ToArrayAsync();

        if (role.RoleType == RoleType.Reseller)
        {
            var resellerRoles = roles.Where(x => x.RoleType == RoleType.Reseller).ToArray();
            UpdateRoles(role, resellerRoles);
        }

        if (role.RoleType == RoleType.Reseller || role.RoleType == RoleType.Licensee)
        {
            var licenseeRootRole = roles.FirstOrDefault(x => x.RoleType == RoleType.Licensee && x.IsRoot);

            if (licenseeRootRole != null)
            {
                UpdateRoles(role,
                    new[]
                    {
                        licenseeRootRole
                    });
            }

            var licenseeRoles = roles.Where(x => x.RoleType == RoleType.Licensee).ToArray();
            UpdateRoles(licenseeRootRole, licenseeRoles);
        }

        var brandRootRole = roles.FirstOrDefault(x => x.RoleType == RoleType.Brand && x.IsRoot);

        if (brandRootRole != null)
        {
            UpdateRoles(role,
                new[]
                {
                    brandRootRole
                });
        }

        var brandRoles = roles.Where(x => x.RoleType == RoleType.Brand).ToArray();
        UpdateRoles(brandRootRole, brandRoles);

        await _roleCommands.BatchUpdateAsync(roles);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpDelete("{roleId}")]
    [Description("Delete a role")]
    public async Task<MessageResponse> DeleteAsync(Guid roleId)
    {
        var role = await _roleQueries.GetSingleAsync(p => p.Id == roleId, s => s.Include(x => x.Users));

        if (role == null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.NotFound);
        }

        if (role.Users.Any())
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.CannotDeleteRole);
        }

        if (role.IsRoot)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.CannotDeleteRootRole);
        }

        if (role.IsRoot)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Role.CannotDeleteRootRole);
        }


        await _roleCommands.DeleteAsync(role);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost("{roleId}/copy")]
    [Description("Copy role")]
    public async Task<MessageResponse> CopyAsync(Guid roleId, [FromBody] CopyRoleRequest request)
    {
        var role = await _roleQueries.GetSingleAsync(p => p.Id == roleId);

        if (role == null)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.NotFound);
        }

        if (role.IsRoot)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.CannotCopyRootRole);
        }

        var roleNameExisted = await _roleQueries.GetQueries()
            .Where(role.ResellerId, x => x.ResellerId == role.ResellerId)
            .Where(role.LicenseeId, x => x.LicenseeId == role.LicenseeId)
            .Where(role.BrandId, x => x.BrandId == role.BrandId)
            .AnyAsync(x => x.Name == request.Name);

        if (roleNameExisted)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.NameExists);
        }

        var newRole = CreateRole(request.Name,
            role.DisplayName,
            role.Description,
            role.ResellerId,
            role.RoleType,
            role.LicenseeId,
            role.BrandId,
            role.MenuSetting,
            role.IsRoot);

        await _roleCommands.CreateAsync(newRole);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost]
    [Description("Create a role")]
    public async Task<MessageResponse> CreateAsync([FromBody] CreateRoleRequest request)
    {
        var roleNameExisted = await _roleQueries.GetQueries()
            .Where(request.ResellerId, x => x.ResellerId == request.ResellerId)
            .Where(request.LicenseeId, x => x.LicenseeId == request.LicenseeId)
            .Where(request.BrandId, x => x.BrandId == request.BrandId)
            .AnyAsync(x => x.Name == request.Name);

        if (roleNameExisted)
        {
            return CreateInvalidResponse<GetRoleResponse>(Resources.Role.NameExists);
        }

        switch (request.IsRoot)
        {
            case true when request.RoleType == RoleType.Brand &&
                await _roleQueries.GetCountAsync(x => x.IsRoot && x.BrandId == request.BrandId) > 0:
                return CreateInvalidResponse<GetRoleResponse>(Resources.Role.BrandRootExisted);

            case true when request.RoleType == RoleType.Licensee &&
                await _roleQueries.GetCountAsync(x
                    => x.IsRoot && x.LicenseeId == request.LicenseeId && x.BrandId == null) >
                0:
                return CreateInvalidResponse<GetRoleResponse>(Resources.Role.LicenseeRootExisted);

            case true when request.RoleType == RoleType.Reseller &&
                await _roleQueries.GetCountAsync(x
                    => x.IsRoot && x.ResellerId == request.ResellerId && x.LicenseeId == null &&
                    x.BrandId == null) >
                0:
                return CreateInvalidResponse<GetRoleResponse>(Resources.Role.ResellerRootExisted);

            case true when request.RoleType == RoleType.Admin && await _roleQueries.GetCountAsync(x =>
                x.IsRoot && x.ResellerId == null && x.LicenseeId == null && x.BrandId == null) > 0:
                return CreateInvalidResponse<GetRoleResponse>(Resources.Role.RootExixted);
        }

        var role = CreateRole(request.Name,
            request.DisplayName,
            request.Description,
            request.ResellerId,
            request.RoleType,
            request.LicenseeId,
            request.BrandId,
            _jsonSerializationProvider.SerializeToString(request.Menus),
            request.IsRoot);

        await _roleCommands.CreateAsync(role);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPost("menus")]
    [Description("Get menus")]
    public async Task<GetMenuResponse> GetMenuAsync(GetMenuRequest request)
    {
        var setting = request.RoleType != RoleType.Admin
            ? (await _roleQueries.GetSingleAsync(x => x.Id == request.RoleId))?.MenuSetting
            : (await _settingQueries.GetSingleAsync(p => p.Key == "Menus"))?.Value;

        if (setting == null)
        {
            return CreateInvalidResponse<GetMenuResponse>(Resources.Role.MenuNotFound);
        }

        var menus = GetMenuSettingByString(setting);

        return new()
        {
            Valid = true,
            Menus = menus
        };
    }

    private Role CreateRole(
        string name, string displayName, string description, Guid? resellerId,
        RoleType roleType, Guid? licenseeId, Guid? brandId, string menuSetting, bool isRoot) =>
        new()
        {
            Id = GuidExtensions.NewSequentialGuid(),
            IsRoot = isRoot,
            Name = name,
            Description = description ?? name,
            DisplayName = displayName ?? name,
            MenuSetting = menuSetting,
            RoleType = roleType,
            ResellerId = resellerId,
            LicenseeId = licenseeId,
            BrandId = brandId,
            CreatedByUsername = Username,
            UpdatedByUsername = Username,
            CreatedOn = DateTimeOffset.Now,
            UpdatedOn = DateTimeOffset.Now
        };

    private MenuSetting[] GetMenuSettingByString(string setting) =>
        _jsonSerializationProvider.DeserializeFromString<MenuSetting[]>(setting);

    private MenuSetting[] GetMenuSetting(string template, string menuSetting)
    {
        var templateMenus = GetMenuSettingByString(template);
        var roleMenus = GetMenuSettingByString(menuSetting);

        foreach (var menu in templateMenus)
        {
            var roleMenuLevel1 = SetMenuDisplay(menu, roleMenus);

            foreach (var subMenu in menu.SubMenus)
            {
                var roleMenuLevel2 = SetMenuDisplay(subMenu, roleMenuLevel1?.SubMenus);

                foreach (var permission in subMenu.SubMenus)
                {
                    SetMenuDisplay(permission, roleMenuLevel2?.SubMenus);
                }
            }
        }

        return templateMenus;
    }

    private static MenuSetting SetMenuDisplay(MenuSetting menuTemplate, IEnumerable<MenuSetting> roleMenus)
    {
        var roleMenuLevel1 = roleMenus?.FirstOrDefault(p => p.Code == menuTemplate.Code);
        menuTemplate.IsDisplay = roleMenuLevel1?.IsDisplay ?? false;

        return roleMenuLevel1;
    }

    private void UpdateRoles(Role templateRole, IEnumerable<Role> roles)
    {
        foreach (var roleItem in roles)
        {
            var menus = GetMenuSetting(templateRole.MenuSetting, roleItem.MenuSetting);
            roleItem.MenuSetting = _jsonSerializationProvider.SerializeToString(menus);
            roleItem.UpdatedByUsername = Username;
            roleItem.UpdatedOn = DateTimeOffset.Now;
        }
    }
}