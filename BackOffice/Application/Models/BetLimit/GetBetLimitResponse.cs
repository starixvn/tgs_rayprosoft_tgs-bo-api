using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitResponse : MessageResponse
{
    public BetLimitResponseModel BetLimit { get; set; }
}
