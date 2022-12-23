using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitsResponse : MessageResponse
{
    public GameProviderBetLimitModel[] BetLimits { get; set; }
}
