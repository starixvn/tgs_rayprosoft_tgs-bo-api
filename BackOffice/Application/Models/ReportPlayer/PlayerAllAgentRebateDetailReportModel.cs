namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class PlayerAllAgentRebateDetailReportModel
{
    public string Username { get; set; } = string.Empty;
    public string PlayerCode { get; set; } = string.Empty;
    public decimal RebateSubTotal { get; set; }
    public decimal SportsbookValidBet { get; set; }
    public decimal SportsbookRebate { get; set; }
    public decimal VirtualSportsValidBet { get; set; }
    public decimal VirtualSportsRebate { get; set; }
    public decimal LiveDealerValidBet { get; set; }
    public decimal LiveDealerRebate { get; set; }
    public decimal CardGameValidBet { get; set; }
    public decimal CardGameRebate { get; set; }
    public decimal LotteryValidBet { get; set; }
    public decimal LotteryRebate { get; set; }
    public decimal SlotValidBet { get; set; }
    public decimal SlotRebate { get; set; }
}
