using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public class UploadSubGameProvidersLocalizationModel
{
    public string Code { get; set; }

    public IDictionary<string, string> Localization { get; set; }

    public string Name { get; set; }
}
