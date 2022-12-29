using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.VipLevel;

public class VipLevelsResponse : MessageResponse
{
    public VipLevelModel[] VipLevels { get; set; }
}
