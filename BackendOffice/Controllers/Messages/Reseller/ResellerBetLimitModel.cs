namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class ResellerBetLimitModel
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; }
    public int MinBet { get; set; }
    public int MaxBet { get; set; }
    public int MaxBetPerMatch { get; set; }
}