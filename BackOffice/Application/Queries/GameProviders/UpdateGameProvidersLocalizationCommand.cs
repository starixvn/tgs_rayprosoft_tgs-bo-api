using System.Collections.Generic;
using TGS.Services.BackOffice.Application.Models.SubGameProvider;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

public record UpdateGameProvidersLocalizationCommand(
    IEnumerable<SubGameProvidersLocalizationModel> SubGameProviders,
    string EditorUserName);
