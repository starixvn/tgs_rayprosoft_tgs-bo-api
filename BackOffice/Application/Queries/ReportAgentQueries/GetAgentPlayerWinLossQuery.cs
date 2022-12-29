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
    GetAgentPlayerWinLossQuery : IGetService<GetAgentPlayerWinLossReportRequest,
        PagedResponse<AgentPlayerWinLossReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetAgentPlayerWinLossQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<AgentPlayerWinLossReportModel>> GetAsync(
        GetAgentPlayerWinLossReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAgentPlayerWinLossRequest
        {
            AgentId = request.AgentId,
            AgentDividendPercentage = request.AgentDividendPercentage,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetAgentPlayerWinLossAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new AgentPlayerWinLossReportModel
            {
                Username = x.Username,
                LiveDealerWinLossAmount = x.LiveDealerWinLossAmount,
                SlotWinLossAmount = x.SlotWinLossAmount,
                LotteryWinLossAmount = x.LotteryWinLossAmount,
                CardGameWinLossAmount = x.CardGameWinLossAmount,
                SportsWinLossAmount = x.SportsWinLossAmount,
                VirtualSportsWinLossAmount = x.VirtualSportsWinLossAmount
            }).ToArray()
        };
    }
}