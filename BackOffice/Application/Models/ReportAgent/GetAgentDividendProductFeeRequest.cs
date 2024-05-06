using System;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public record GetAgentDividendProductFeeRequest
{
    public Guid AgentId { get; init; }
    public decimal AgentDividendPercentage { get; init; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}