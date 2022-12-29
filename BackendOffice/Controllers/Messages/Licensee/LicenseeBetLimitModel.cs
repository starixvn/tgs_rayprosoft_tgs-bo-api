namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class LicenseeBetLimitModel
{
    public string CurrencyCode { get; set; }

    public int Id { get; set; }

    public int MaxBet { get; set; }

    public int MaxBetPerMatch { get; set; }

    public int MinBet { get; set; }
}
