namespace TGS.Services.BackOffice.Application.Models.Game;

public class ExportGameModel
{
    public string AvailableCurrencies { get; set; }

    public bool? AvailableToBrand { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    public string EndpointPath { get; set; }

    public string ExternalId { get; set; }

    public bool? HasTurnoverOnTieBet { get; set; }

    public bool? Html5 { get; set; }

    public string Name { get; set; }

    public string PlatformType { get; set; }

    public string RuleUrl { get; set; }

    public bool? SubGame { get; set; }

    public string SubGpCode { get; set; }

    public string Type { get; set; }
}
