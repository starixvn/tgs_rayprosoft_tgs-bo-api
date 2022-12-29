using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentContributionsReportResponse : MessageResponse
{
    public AgentContributionReportModel[] Items { get; init; }

    public AgentContributionTotalModel Total { get; init; }

    public int TotalCount { get; init; }
}
