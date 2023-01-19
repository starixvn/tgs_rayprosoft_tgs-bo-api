#nullable enable

using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Domain;

public record GameInfo(
    string Name,
    string Code,
    string RuleUrl,
    string ExternalId,
    GameType Type,
    GamePlatformType PlatformType,
    bool HasTurnoverOnTieBet,
    string EndpointPath,
    string Description,
    bool SubGame,
    bool AvailableToBrand,
    bool Html5,
    string AvailableCurrencies,
    string SubGameProviderCode);
