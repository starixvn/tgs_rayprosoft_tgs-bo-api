using System;
using System.ComponentModel;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public record GetRoundHistoryReportRequest
{
    public Guid RoundId { get; set; }

    public string GameProviderCode { get; set; }

    public string PlayerCode { get; set; }

    public string ExternalId { get; set; }

    [DefaultValue(typeof(string), "en-US")]
    public string Language { get; set; }

    [DefaultValue(typeof(string), null)]
    public string BaseCurrency { get; set; }

    [DefaultValue(typeof(string), null)]
    public string PlayerUsername { get; set; }
}