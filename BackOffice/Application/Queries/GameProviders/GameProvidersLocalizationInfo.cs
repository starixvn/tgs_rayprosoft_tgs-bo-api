using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public class GameProvidersLocalizationInfo
{
    public IEnumerable<GameProviderData> GameProviders { get; set; }

    public IEnumerable<string> Languages { get; set; }
}
