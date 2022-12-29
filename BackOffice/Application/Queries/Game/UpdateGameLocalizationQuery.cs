using System.Collections.Generic;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.Game;

public class UpdateGameLocalizationQuery
{
    public string EditorUsername { get; set; }

    public List<GameLocalizationModel> Games { get; set; }
}
