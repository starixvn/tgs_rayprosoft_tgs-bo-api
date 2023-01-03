using System;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayerOperationModel
{
    public string CreatedByUsername { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string Operation { get; init; }
}
