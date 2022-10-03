namespace TGS.Services.BackOffice.Application.Models.Player;

public record GetPlayerGameProviderBalanceResponse(
    string GameProviderCode,
    string GameProviderName,
    bool IsMainWallet = false,
    bool IsMaintain = true,
    decimal? Balance = null);