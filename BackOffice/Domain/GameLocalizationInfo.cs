using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;

public class GameLocalizationInfo
{
    public IEnumerable<GameData> Games { get; set; }

    public IEnumerable<string> Languages { get; set; }
}
