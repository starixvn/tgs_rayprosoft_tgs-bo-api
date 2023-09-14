namespace TGS.Services.BackOffice.Domain;

public record TopTenPlayerInfo
{
    public string PlayerUserName { get; set; }
    public decimal DepositTotalAmount { get; set; }
    public decimal WithdrawTotalAmount { get; set; }
    public decimal Bet { get; set; }
    public decimal Settle { get; set; }
    public decimal WinLoss { get; set; }
};
