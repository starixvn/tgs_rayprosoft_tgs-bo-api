using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentDividendsReportResponse : MessageResponse
{
    public int TotalCount { get; init; }

    public AgentDividendReportModel[] Items { get; init; }

    public AgentDividendReportTotalModel Total { get; init; }
}