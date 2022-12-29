using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;

namespace TGS.Services.BackOffice.Application.Models.GameProvider;

public class GameProviderModel : GameProviderBaseModel
{
    public bool AllowKickPlayer { get; set; }

    public bool AutoCloseRound { get; set; }

    public GameProviderBetLimitModel[] BetLimits { get; set; }

    public GameProviderCurrencyModel[] Currencies { get; set; }

    public bool IsTransferWallet { get; set; }

    public GameProviderLanguageModel[] Languages { get; set; }

    public int PlaceBetTimeout { get; set; }

    public int SettleBetTimeout { get; set; }

    public bool SupportTestPlayer { get; set; }

    public ushort UniqueId { get; set; }
}
