using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public record GameProvidersBrandQueryResult
{
    public IEnumerable<GameProvidersBrandQueryGameProviderData> GameProviders { get; set; }

    public IEnumerable<GameProvidersBrandQueryBrandData> GameProvidersBrands { get; set; }
}
