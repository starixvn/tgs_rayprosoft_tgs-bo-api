using TGS.Core.Models.Messages;
using TGS.Core.Models.Services;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class GetMenuResponse : MessageResponse
{
    public MenuSetting[] Menus { get; set; }
}