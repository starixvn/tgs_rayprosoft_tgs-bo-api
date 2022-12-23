using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentQueries;

public class
    GetAgentDividendDetailsQuery : IGetService<GetAgentDividendDetailsReportRequest,
        PagedResponse<AgentDividendDetailReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetAgentDividendDetailsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<AgentDividendDetailReportModel>> GetAsync(
        GetAgentDividendDetailsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAgentDividendDetailsRequest
        {
            AgentId = request.AgentId,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetAgentDividendDetailsAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new AgentDividendDetailReportModel
            {
                Id = x.Id,
                Username = x.Username,
                SettlementDate = x.SettlementDate,
                SettlementDateFrom = x.SettlementDateFrom,
                SettlementDateTo = x.SettlementDateTo,
                LiveDealerWinLoss = x.LiveDealerWinLoss,
                SlotWinLoss = x.SlotWinLoss,
                LotteryWinLoss = x.LotteryWinLoss,
                CardGameWinLoss = x.CardGameWinLoss,
                SportsWinLoss = x.SportsWinLoss,
                VirtualSportsWinLoss = x.VirtualSportsWinLoss,
                WinLossAmount = x.WinLossAmount,
                AgentDividendAmount = x.AgentDividendAmount,
                AgentDividendPercentage = x.AgentDividendPercentage,
                ExpenditureAmount = x.ExpenditureAmount
            }).ToArray()
        };
    }
}
