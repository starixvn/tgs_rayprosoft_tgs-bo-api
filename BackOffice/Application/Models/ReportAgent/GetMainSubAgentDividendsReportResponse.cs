using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetMainSubAgentDividendsReportResponse : MessageResponse
{
    public AgentDividendReportModel[] MainItems { get; init; }

    public AgentDividendReportModel[] SubItems { get; init; }

    public AgentDividendReportTotalModel SubTotal { get; init; }

    public int SubTotalCount { get; init; }
}
