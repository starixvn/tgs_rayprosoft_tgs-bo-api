namespace TGS.Services.BackOffice.Application.Queries.PlayerHandlers;

public record FundInPlayerGameProviderCommand(
    string PlayerCode,
    string GameProviderCode);
