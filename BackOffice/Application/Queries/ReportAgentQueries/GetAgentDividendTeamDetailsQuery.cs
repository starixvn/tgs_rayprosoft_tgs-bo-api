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
    GetAgentDividendTeamDetailsQuery : IGetService<GetAgentDividendTeamDetailsReportRequest,
        PagedResponse<AgentDividendTeamDetailReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetAgentDividendTeamDetailsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<AgentDividendTeamDetailReportModel>> GetAsync(
        GetAgentDividendTeamDetailsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAgentDividendTeamDetailsRequest
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

        var response = await _reportClient.GetAgentDividendTeamDetailsAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new AgentDividendTeamDetailReportModel
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
                ExpenditureAmount = x.ExpenditureAmount,
                BonusAmount = x.BonusAmount,
                NetProfit = x.NetProfit,
                CompanyWinLossTotalAmount = x.CompanyWinLossTotalAmount
            }).ToArray()
        };
    }
}