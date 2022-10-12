using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.LicenseeMarket;
using TGS.Services.BackOffice.Application.Models.ReportLicenseeMarket;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportLicenseeMarketQueries;

public class
    GetLicenseeMarketsQuery : IGetService<GetLicenseeMarketsReportRequest,
        LicenseeMarketsReportResponse>
{
    private readonly IReportClient _reportClient;

    public GetLicenseeMarketsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<LicenseeMarketsReportResponse> GetAsync(
        GetLicenseeMarketsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetLicenseeMarketsRequest
        {
            CurrencyCode = request.CurrencyCode,
            CurrencyCodes = request.CurrencyCodes!,
            LicenseeId = request.LicenseeId,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetLicenseeMarketsAsync(reportRequest);

        return new()
        {
            Datas = new()
            {
                TotalCount = response.Datas.TotalCount,
                Items = response.Datas.Items.Select(x => new LicenseeMarketDataReportModel
                {
                    BrandId = x.BrandId,
                    BrandCode = x.BrandCode,
                    CurrencyCode = x.CurrencyCode,
                    RegisteredCount = x.RegisteredCount,
                    FirstFundInCount = x.FirstFundInCount,
                    DepositAmount = x.DepositAmount,
                    WithdrawalAmount = x.WithdrawalAmount,
                    CompanyAdjustAmount = x.CompanyAdjustAmount,
                    BonusAmount = x.BonusAmount,
                    BetAmount = x.BetAmount,
                    Turnover = x.Turnover,
                    WinLossAmount = x.WinLossAmount,
                    NetProfit = x.NetProfit,
                    DepositHandling = 0
                }).ToArray()
            },
            Totals = response.Totals.Select(x => new LicenseeMarketTotalReportModel
            {
                CurrencyCode = x.CurrencyCode,
                RegisteredCountSummary = x.RegisteredCount,
                FirstFundInCountSummary = x.FirstFundInCount,
                DepositAmountSummary = x.DepositAmount,
                WithdrawalAmountSummary = x.WithdrawalAmount,
                CompanyAdjustAmountSummary = x.CompanyAdjustAmount,
                BonusAmountSummary = x.BonusAmount,
                BetAmountSummary = x.BetAmount,
                TurnoverSummary = x.Turnover,
                WinLossAmountSummary = x.WinLossAmount,
                NetProfitSummary = x.NetProfit,
                DepositHandlingFeeSummary = 0
            }).ToArray()
        };
    }
}
