using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GameProviderModel : GameProviderBaseModel
{
    public bool AutoCloseRound { get; set; }
    public bool SupportTestPlayer { get; set; }
    public bool IsTransferWallet { get; set; }
    public bool AllowKickPlayer { get; set; }
    public int PlaceBetTimeout { get; set; }
    public int SettleBetTimeout { get; set; }
    public ushort UniqueId { get; set; }
    public GameProviderCurrencyModel[] Currencies { get; set; }
    public GameProviderLanguageModel[] Languages { get; set; }
    public GameProviderBetLimitModel[] BetLimits { get; set; }
}