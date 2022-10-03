using System;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayerOperationModel
{
    public DateTimeOffset CreatedOn { get; init; }
    public string Operation { get; init; }
    public string CreatedByUsername { get; init; }
}