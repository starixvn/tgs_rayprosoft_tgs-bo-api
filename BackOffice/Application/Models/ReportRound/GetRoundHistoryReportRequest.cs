using System;

namespace TGS.Services.BackOffice.Application.Models.ReportRound;

public record GetRoundHistoryReportRequest
{
    public string GameProviderCode { get; set; }

    public string PlayerCode { get; set; }

    public string Language { get; set; }

    public Guid RoundId { get; set; }
}
