using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class GetBetLimitResponse : MessageResponse
{
    public BetLimitResponseModel BetLimit { get; set; }
}
