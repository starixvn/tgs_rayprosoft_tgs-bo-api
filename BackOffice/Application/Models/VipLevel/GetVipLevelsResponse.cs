using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.VipLevel;

public class GetVipLevelsResponse : MessageResponse
{
    public VipLevelModel[] VipLevels { get; init; }
}
