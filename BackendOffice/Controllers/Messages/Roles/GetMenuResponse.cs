using TGS.Core.Models.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class GetMenuResponse : MessageResponse
{
    public MenuSetting[] Menus { get; set; }
}
