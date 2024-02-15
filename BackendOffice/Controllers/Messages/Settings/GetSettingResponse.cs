using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Settings;

public class GetSettingResponse : MessageResponse
{
    public SettingModel Setting { get; set; }
}
