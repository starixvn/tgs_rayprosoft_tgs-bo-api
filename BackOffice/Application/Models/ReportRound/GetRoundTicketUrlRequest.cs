using System.ComponentModel;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public record GetRoundTicketUrlRequest
{
    public required string GameProviderCode { get; set; }

    public required string PlayerCode { get; set; }

    public required long ExternalId { get; set; }

    [DefaultValue(typeof(string), null)]
    public string BaseCurrency { get; set; }

    [DefaultValue(typeof(string), "en-US")]
    public string Language { get; set; }

    [DefaultValue(typeof(string), null)]
    public string PlayerUsername { get; set; }
}