using System.Collections.Generic;

namespace TGS.Services.BackOffice.Application.Models.SubGameProvider;

public class UpdateSubGameProvidersLocalizationRequest
{
    public List<SubGameProvidersLocalizationModel> SubGameProviders { get; set; }
    public string EditorUsername { get; set; }
}