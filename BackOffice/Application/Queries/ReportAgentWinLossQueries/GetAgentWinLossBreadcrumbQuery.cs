using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Models.Enums;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.AgentWinLoss;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentWinLossBreadcrumbQuery : IGetService<GetAgentWinLossBreadcrumbReportRequest,
        IEnumerable<AgentWinLossBreadcrumbReportModel>>
{
    private readonly IAgentService _agentService;
    private readonly IPlayerService _playerService;
    private readonly IReportClient _reportClient;

    public GetAgentWinLossBreadcrumbQuery(
        IReportClient reportClient,
        IAgentService agentService,
        IPlayerService playerService)
    {
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));
        _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
    }

    public async ValueTask<IEnumerable<AgentWinLossBreadcrumbReportModel>> GetAsync(
        GetAgentWinLossBreadcrumbReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var agentId = !string.IsNullOrEmpty(request.Username) && request.MemberType == AccountType.Agent
            ? await _agentService.GetAgentQueries()
                .Where(x => x.BrandId == request.BrandId && x.Username == request.Username)
                .Select(x => x.Id).FirstOrDefaultAsync(cancellationToken)
            : (Guid?)null;

        var playerCode = !string.IsNullOrEmpty(request.Username) && request.MemberType == AccountType.Player
            ? await _playerService.GetPlayerQueries()
                .Where(x => x.BrandId == request.BrandId && x.Username == request.Username)
                .Select(x => x.PlayerCode)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var reportRequest = new GetBreadcrumbRequest
        {
            AgentId = agentId,
            PlayerCode = playerCode,
            RoundId = request.RoundId
        };

        var response = await _reportClient.GetAgentWinLossBreadcrumbAsync(reportRequest);

        return response.Select(x => new AgentWinLossBreadcrumbReportModel
        {
            UserName = x.Name,
            Id = x.Id,
            MemberType = x.Type
        });
    }
}