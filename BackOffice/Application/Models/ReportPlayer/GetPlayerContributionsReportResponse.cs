using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportPlayer;

public class GetPlayerContributionsReportResponse : MessageResponse
{
    public int TotalCount { get; init; }

    public PlayerContributionReportModel[] Items { get; init; }

    public PlayerContributionReportTotalModel Total { get; init; }
}