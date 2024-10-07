#nullable enable
using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;

public class GameLocalizationModel
{
    public string Code { get; set; } = string.Empty;

    public int Id { get; set; }

    public IDictionary<string, GameLocalizationDetailModel> Localization { get; set; }
}

public class GameLocalizationDetailModel
{
    public string Name { get; set; } = string.Empty;
}
