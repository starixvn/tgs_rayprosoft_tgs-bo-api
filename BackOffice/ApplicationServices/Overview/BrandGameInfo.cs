using System;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record BrandGameInfo(
    int GameId,
    string GameName,
    string Localization,
    Guid GameProviderId,
    string GameProviderName);
