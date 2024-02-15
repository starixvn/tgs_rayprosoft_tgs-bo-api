using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

public class GetGameProviderBetLimitResponse : MessageResponse
{
    public GameProviderBetLimitModel GameProviderBetLimit { get; set; }
}
