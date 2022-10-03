using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitResponse : MessageResponse
{
    public GameProviderBetLimitModel GameProviderBetLimit { get; set; }
}