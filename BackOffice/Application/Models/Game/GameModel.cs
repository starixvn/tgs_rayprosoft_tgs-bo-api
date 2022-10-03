using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Game;

public class GameModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public GameStatus Status { get; set; }
    public string ExternalId { get; set; }
    public string Description { get; set; }
    public string EndpointPath { get; set; }
    public string RuleUrl { get; set; }
    public GameType Type { get; set; }
    public GamePlatformType PlatformType { get; set; }
    public bool SubGame { get; set; }
    public bool AvailableToBrand { get; set; }
    public bool HasTurnoverOnTieBet { get; set; }
    public bool Html5 { get; set; }
    public string AvailableCurrencies { get; set; } = "";
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public string CreatedByUsername { get; set; }
    public string UpdatedByUsername { get; set; }
    public string SubGpCode { get; set; }
}