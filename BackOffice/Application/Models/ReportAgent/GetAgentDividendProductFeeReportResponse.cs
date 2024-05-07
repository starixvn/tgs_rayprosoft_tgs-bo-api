using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentDividendProductFeeReportResponse : MessageResponse
{
    public decimal LiveDealerAmount { get; init; }
    public decimal SlotAmount { get; init; }
    public decimal LotteryAmount { get; init; }
    public decimal CardGameAmount { get; init; }
    public decimal SportsAmount { get; init; }
    public decimal VirtualSportsAmount { get; init; }
}
