using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentQueries;

public class
    GetMainSubAgentDividendsQuery(IReportClient reportClient) : IGetService<GetMainSubAgentDividendsReportRequest, GetMainSubAgentDividendsReportResponse>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetMainSubAgentDividendsReportResponse> GetAsync(GetMainSubAgentDividendsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var singleRequest = new GetSingleAgentDividendsRequest
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

        var mainResponse = await _reportClient.GetSingleAgentDividendsAsync(singleRequest);

        if (!mainResponse.Valid)
        {
            return new GetMainSubAgentDividendsReportResponse
            {
                Valid = mainResponse.Valid,
                Message = mainResponse.Message,
                SubTotalCount = mainResponse.TotalCount,
                MainItems = [],
                SubItems = [],
                SubTotal = new AgentDividendReportTotalModel(
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0)
            };
        }

        var subRequest = new GetSubAgentDividendsRequest
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

        var subResponse = await _reportClient.GetSubAgentDividendsAsync(subRequest);

        if (!subResponse.Valid)
        {
            return new GetMainSubAgentDividendsReportResponse
            {
                Valid = subResponse.Valid,
                Message = subResponse.Message,
                SubTotalCount = subResponse.TotalCount,
                MainItems = [],
                SubItems = [],
                SubTotal = new AgentDividendReportTotalModel(
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0)
            };
        }

        var pageArray = subResponse.Items.Select(x => new AgentDividendReportModel
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
        }).AsQueryable().CreatePagedResponse(new PageableRequest
        {
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy
        });

        var subTotal = new AgentDividendReportTotalModel(
            subResponse.Items.Sum(x => x.BonusAdjustAmount),
            subResponse.Items.Sum(x => x.AgentAdjustAmount),
            subResponse.Items.Sum(x => x.BonusAmount),
            subResponse.Items.Sum(x => x.BetAmount),
            subResponse.Items.Sum(x => x.ValidBetAmount),
            subResponse.Items.Sum(x => x.Turnover),
            subResponse.Items.Sum(x => x.WinLossAmount),
            subResponse.Items.Sum(x => x.NetProfit),
            subResponse.Items.Sum(x => x.AgentDividendAmount),
            subResponse.Items.Sum(x => x.FromPlayerAgentDividendAmount),
            subResponse.Items.Sum(x => x.FromTeamAgentDividendAmount),
            subResponse.Items.Sum(x => x.AgentCoveredBonus) ?? 0M,
            subResponse.Items.Sum(x => x.AgentCoveredBonusAdjust) ?? 0M
        );

        return new GetMainSubAgentDividendsReportResponse
        {
            Valid = true,
            Message = "",
            SubTotalCount = pageArray.TotalCount,
            MainItems = mainResponse.Items.Select(x => new AgentDividendReportModel
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
            }).ToArray(),
            SubItems = pageArray.Items,
            SubTotal = subTotal
        };
    }
}
