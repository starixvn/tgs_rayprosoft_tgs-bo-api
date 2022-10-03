using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class SubGameProvidersLocalizationInfo
{
    public IEnumerable<string> Languages { get; set; }

    public IEnumerable<SubGameProviderData> GameProviders { get; set; }
}