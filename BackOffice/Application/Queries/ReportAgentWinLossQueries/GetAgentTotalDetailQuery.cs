using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentTotalDetailQuery(
    IReportClient reportClient) : IGetService<GetAgentTotalDetailReportRequest,
        IEnumerable<AgentTotalDetailReportModel>>
{
    public async ValueTask<IEnumerable<AgentTotalDetailReportModel>> GetAsync(
        GetAgentTotalDetailReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await reportClient.GetAgentTotalDetailAsync(new()
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            From = request.From,
            To = request.To
        });

        return response.Select(x => new AgentTotalDetailReportModel
        {
            From = x.From,
            To = x.To,
            Percentage = x.Percentage,
            PlayerWinLoss = x.PlayerWinLoss,
            PlayerSpecial = x.PlayerSpecial,
            AgentWinLoss = x.AgentWinLoss,
            AgentSpecial = x.AgentSpecial,
            PlatformChargeAmount = x.PlatformChargeAmount,
            AgentTotal = x.AgentTotal
        });
    }
}