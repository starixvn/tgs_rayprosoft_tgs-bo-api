using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public record TeamInfoReportModel
{
    public string Username { get; init; }

    public DateTimeOffset RegisteredOn { get; init; }

    public decimal Contribution { get; init; }
}
