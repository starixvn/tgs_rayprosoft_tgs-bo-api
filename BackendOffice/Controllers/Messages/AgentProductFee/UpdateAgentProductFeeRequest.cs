
using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.AgentProductFee;

public record UpdateAgentProductFeeRequest
{
	public Guid AgentId { get; init; }
    public decimal CurrentCardGamePercentage { get; init; }
    public decimal CurrentLiveDealerPercentage { get; init; }
    public decimal CurrentLotteryPercentage { get; init; }
    public decimal CurrentSlotPercentage { get; init; }
    public decimal CurrentSportsPercentage { get; init; }
    public decimal CurrentVirtualSportsPercentage { get; init; }
    public decimal NextCardGamePercentage { get; init; }
    public decimal NextLiveDealerPercentage { get; init; }
    public decimal NextLotteryPercentage { get; init; }
    public decimal NextSlotPercentage { get; init; }
    public decimal NextSportsPercentage { get; init; }
    public decimal NextVirtualSportsPercentage { get; init; }
}
