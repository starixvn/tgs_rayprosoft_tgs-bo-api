using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Settings;

public class GetSettingsRequest : PageableRequest
{
    public string Key { get; set; }
}
