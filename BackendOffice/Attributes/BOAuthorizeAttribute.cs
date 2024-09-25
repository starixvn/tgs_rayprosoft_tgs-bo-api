using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Attributes
{
    public class BOAuthorizeAttribute : AuthorizeAttribute, IAuthorizationRequirement,
                                     IAuthorizationRequirementData
    {
        public string[] UserRoles { get; }

        public BOAuthorizeAttribute(string[] roles) => UserRoles = roles;

        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            yield return this;
        }
    }

    public class BOAuthorizationHandler : AuthorizationHandler<BOAuthorizeAttribute>
    {
        private readonly ILogger<BOAuthorizationHandler> _logger;
        private readonly IMenuSettingRepository _menuSettingRepository;
        private readonly IJsonSerializationProvider _jsonSerializationProvider;

        public BOAuthorizationHandler(ILogger<BOAuthorizationHandler> logger, IMenuSettingRepository menuSettingRepository, IJsonSerializationProvider jsonSerializationProvider)
        {
            _logger = logger;
            _menuSettingRepository = menuSettingRepository;
            _jsonSerializationProvider = jsonSerializationProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   BOAuthorizeAttribute requirement)
        {
            var roleIdClaim = context.User.FindFirst(c => c.Type == "UserRoleId");
            if (roleIdClaim != null)
            {
                if (await HasPermission(roleIdClaim.Value, requirement.UserRoles))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogInformation("No Permission");
                }
            }
            else
            {
                _logger.LogInformation("No RoleId claim present");
            }
        }

        private async ValueTask<bool> HasPermission(string roleId, string[] roles)
        {
            var menuSettingsString = await _menuSettingRepository.GetAsync(roleId);
            var menuSettings = _jsonSerializationProvider.DeserializeFromString<MenuSetting[]>(menuSettingsString);
            foreach (var role in roles)
            {
                if (HasMenuSetting(menuSettings, role))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasMenuSetting(MenuSetting[] menuSettings, string role)
        {
            var userRoles = role.Split('.');

            MenuSetting currentMenu = null;
            var subMenuSetting = menuSettings;
            foreach (var userRole in userRoles)
            {
                currentMenu = FindMenuSetting(subMenuSetting, userRole);

                if (currentMenu == null)
                {
                    return false;
                }
                subMenuSetting = currentMenu.SubMenus;
            }

            return currentMenu?.IsDisplay == true;
        }

        private MenuSetting? FindMenuSetting(MenuSetting[] menu, string code)
        {
            return menu.FirstOrDefault(x => x.Code.ToLowerInvariant() == code.ToLowerInvariant() && x.IsDisplay);
        }
    }
}