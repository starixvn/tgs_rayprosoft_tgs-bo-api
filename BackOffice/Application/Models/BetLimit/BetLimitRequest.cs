namespace TGS.Services.BackOffice.Application.Models.BetLimit;

public class BetLimitRequest : BaseRequest
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; }
    public int MinBet { get; set; }
    public int MaxBet { get; set; }
    public int MaxBetPerMatch { get; set; }
}