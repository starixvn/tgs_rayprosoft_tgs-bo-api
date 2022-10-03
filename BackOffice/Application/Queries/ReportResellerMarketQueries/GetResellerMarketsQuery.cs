using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.Core.Models.Messages;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.ResellerMarket;
using TGS.Services.BackOffice.Application.Models.ReportResellerMarket;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportResellerMarketQueries;

public class
    GetResellerMarketsQuery : IGetService<GetResellerMarketsReportRequest,
        ResellerMarketsReportResponse>
{
    private readonly IReportClient _reportClient;

    public GetResellerMarketsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<ResellerMarketsReportResponse> GetAsync(
        GetResellerMarketsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetResellerMarketsRequest
        {
            CurrencyCode = request.CurrencyCode,
            CurrencyCodes = request.CurrencyCodes,
            ResellerId = request.ResellerId,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetResellerMarketsAsync(reportRequest).ConfigureAwait(false);

        return new()
        {
            Datas = new PagedResponse<ResellerMarketDataReportModel>
            {
                TotalCount = response.Datas.TotalCount,
                Items = response.Datas.Items.Select(x => new ResellerMarketDataReportModel
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
                    NetProfit = x.NetProfit
                }).ToArray()
            },
            Totals = response.Totals.Select(x => new ResellerMarketTotalReportModel
            {
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
                NetProfit = x.NetProfit
            }).ToArray()
        };
    }
}

