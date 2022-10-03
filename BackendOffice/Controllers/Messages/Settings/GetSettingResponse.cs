using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Settings;

public class GetSettingResponse : MessageResponse
{
    public SettingModel Setting { get; set; }
}
