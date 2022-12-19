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
    GetSingleAgentDividendsQuery : IGetService<GetSingleAgentDividendsReportRequest, PagedResponse<AgentDividendReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetSingleAgentDividendsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<AgentDividendReportModel>> GetAsync(GetSingleAgentDividendsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetSingleAgentDividendsRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            Username = request.Username,
            Type = (short?)request.Type,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = true
        };

        var response = await _reportClient.GetSingleAgentDividendsAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items.Select(x => new AgentDividendReportModel
            {
                Id = x.Id,
                Username = x.Username,
                BonusAdjustAmount = x.BonusAdjustAmount,
                AgentAdjustAmount = x.AgentAdjustAmount,
                BonusAmount = x.BonusAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                Turnover = x.Turnover,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit,
                AgentDividendAmount = x.AgentDividendAmount,
                FromPlayerAgentDividendAmount = x.FromPlayerAgentDividendAmount,
                FromTeamAgentDividendAmount = x.FromTeamAgentDividendAmount,
                AgentDividendPercentage = x.AgentDividendPercentage,
                AgentCoveredBonus = x.AgentCoveredBonus,
                AgentCoveredBonusAdjust = x.AgentCoveredBonusAdjust
            }).ToArray()
        };
    }
}
