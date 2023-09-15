namespace TGS.Services.BackOffice.Domain;

public record TopTenGameInfo
{
    public string GameProviderName { get; set; }
    public string GameName { get; set; }
    public decimal Bet { get; set; }
    public decimal WinLoss { get; set; }
}
