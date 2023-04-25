using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using System.Collections.Generic;
using Tgs.Report.Client.Models.AgentWinLoss;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentWinLossBreadcrumbQuery : IGetService<GetAgentWinLossBreadcrumbReportRequest, IEnumerable<AgentWinLossBreadcrumbReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetAgentWinLossBreadcrumbQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<IEnumerable<AgentWinLossBreadcrumbReportModel>> GetAsync(GetAgentWinLossBreadcrumbReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetBreadcrumbRequest
        {
            AgentId = request.AgentId,
            PlayerCode = request.PlayerCode,
            RoundId = request.RoundId
        };

        var response = await _reportClient.GetAgentWinLossBreadcrumbAsync(reportRequest);

        return response.Select(x => new AgentWinLossBreadcrumbReportModel
        {
            Name = x.Name,
            Id = x.Id,
            Type = x.Type,
        });
    }
}
