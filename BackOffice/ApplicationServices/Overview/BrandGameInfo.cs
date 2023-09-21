using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record BrandGameInfo
{
    public int GameId { get; set; }
    public string GameName { get; set; }
    public Dictionary<string, GameNameLocalization> LocalizationDicts { get; set; }
    public Guid GameProviderId { get; set; }
    public string GameProviderName { get; set; }
}
