namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public class BetRecordTotalView
{
    public decimal BetAmount { get; set; }

    public decimal ValidBetAmount { get; set; }

    public decimal SettleAmount { get; set; }

    public decimal WinLoss { get; set; }
}