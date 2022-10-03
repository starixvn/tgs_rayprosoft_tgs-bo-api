using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public record GameProvidersBrandQueryBrandData(Guid BrandId, string BrandName, string BrabdCode)
{
    public Dictionary<string, bool> GameProvider { get; set; }
}
