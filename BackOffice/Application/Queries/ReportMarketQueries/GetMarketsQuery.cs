using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Market;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportMarket;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketQueries;

public class
    GetMarketsQuery(IReportClient reportClient) : IGetService<GetMarketsReportRequest,
        GetMarketsReportResponse>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetMarketsReportResponse> GetAsync(
        GetMarketsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetMarketsRequest
        {
            BrandId = request.BrandId,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = true
        };

        var response = await _reportClient.GetMarketsAsync(reportRequest).ConfigureAwait(false);

        if (!response.Valid)
        {
            return new GetMarketsReportResponse
            {
                Valid = response.Valid,
                Message = response.Message,
                TotalCount = response.TotalCount,
                Items = [],
                Total = new MarketReportTotalModel(
                    RegisteredCount: 0,
                    FirstFundInCount: 0,
                    DepositCount: 0,
                    ActivePlayer: 0,
                    LoginPlayer: 0,
                    DepositAmountSummary: 0,
                    DepositHandlingFeeSummary: 0,
                    WithdrawalAmountSummary: 0,
                    CompanyAdjustAmountSummary: 0,
                    BonusAdjustAmountSummary: 0,
                    BonusAmountSummary: 0,
                    BetAmountSummary: 0,
                    ValidBetAmountSummary: 0,
                    TurnoverSummary: 0,
                    WinLossAmountSummary: 0,
                    NetProfitSummary: 0,
                    AdditionalExpensesSummary: 0,
                    PlatformCoveredBonusSummary: 0,
                    PlatformCoveredBonusAdjustSummary: 0,
                    AgentProfitSummary: 0)
            };
        }

        var pageArray = response.Items?.Select(x
            => new MarketReportModel
            {
                ReportDate = x.ReportDate,
                RegisteredCount = x.RegisteredCount,
                FirstFundInCount = x.FirstFundInCount,
                DepositCount = x.DepositCount,
                ActivePlayer = x.ActivePlayer,
                LoginPlayer = x.LoginPlayer,
                DepositAmount = x.DepositAmount,
                DepositHandlingFee = x.DepositHandlingFee,
                WithdrawalAmount = x.WithdrawalAmount,
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                BonusAdjustAmount = x.BonusAdjustAmount,
                BonusAmount = x.BonusAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                Turnover = x.Turnover,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit,
                AdditionalExpenses = x.AdditionalExpenses,
                PlatformCoveredBonus = x.PlatformCoveredBonus,
                PlatformCoveredBonusAdjust = x.PlatformCoveredBonusAdjust,
                AgentProfit = x.AgentProfit
            })
            .AsQueryable()
            .CreatePagedResponse(
                new PageableRequest
                {
                    Page = request.Page,
                    Rows = request.Rows,
                    OrderBy = request.OrderBy,
                    SortBy = request.SortBy
                });

        var total = new MarketReportTotalModel(
            RegisteredCount: response.Items.Sum(x => x.RegisteredCount),
            FirstFundInCount: response.Items.Sum(x => x.FirstFundInCount),
            DepositCount: response.Items.Sum(x => x.DepositCount),
            ActivePlayer: response.Items.Sum(x => x.ActivePlayer),
            LoginPlayer: response.Items.Sum(x => x.LoginPlayer),
            DepositAmountSummary: response.Items.Sum(x => x.DepositAmount),
            DepositHandlingFeeSummary: response.Items.Sum(x => x.DepositHandlingFee),
            WithdrawalAmountSummary: response.Items.Sum(x => x.WithdrawalAmount),
            CompanyAdjustAmountSummary: response.Items.Sum(x => x.CompanyAdjustAmount),
            BonusAdjustAmountSummary: response.Items.Sum(x => x.BonusAdjustAmount),
            BonusAmountSummary: response.Items.Sum(x => x.BonusAmount),
            BetAmountSummary: response.Items.Sum(x => x.BetAmount),
            ValidBetAmountSummary: response.Items.Sum(x => x.ValidBetAmount),
            TurnoverSummary: response.Items.Sum(x => x.Turnover),
            WinLossAmountSummary: response.Items.Sum(x => x.WinLossAmount),
            NetProfitSummary: response.Items.Sum(x => x.NetProfit),
            AdditionalExpensesSummary: response.Items.Sum(x => x.AdditionalExpenses),
            PlatformCoveredBonusSummary: response.Items.Sum(x => x.PlatformCoveredBonus),
            PlatformCoveredBonusAdjustSummary: response.Items.Sum(x => x.PlatformCoveredBonusAdjust),
            AgentProfitSummary: response.Items.Sum(x => x.AgentProfit));

        return new GetMarketsReportResponse
        {
            TotalCount = response.TotalCount,
            Items = pageArray.Items,
            Total = total
        };
    }
}
