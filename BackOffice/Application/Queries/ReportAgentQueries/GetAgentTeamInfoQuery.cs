using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentQueries;

public class
    GetAgentTeamInfoQuery : IGetService<GetAgentTeamInfoReportRequest, PagedResponse<TeamInfoReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetAgentTeamInfoQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<TeamInfoReportModel>> GetAsync(GetAgentTeamInfoReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAgentTeamInfoRequest
        {
            AgentId = request.AgentId,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetAgentTeamInfoAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new TeamInfoReportModel
            {
                Username = x.Username,
                RegisteredOn = x.RegisteredOn,
                Contribution = x.Contribution
            }).ToArray()
        };
    }
}