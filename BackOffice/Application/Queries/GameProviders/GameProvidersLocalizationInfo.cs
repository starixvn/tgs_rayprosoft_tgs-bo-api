using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public class GameProvidersLocalizationInfo
{
    public IEnumerable<string> Languages { get; set; }

    public IEnumerable<GameProviderData> GameProviders { get; set; }
}