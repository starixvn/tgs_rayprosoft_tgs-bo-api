using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class UpdateSubGameProvidersLocalizationRequest
{
    public string EditorUsername { get; set; }

    public List<SubGameProvidersLocalizationModel> SubGameProviders { get; set; }
}
