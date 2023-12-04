using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Vip;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportVip;

namespace TGS.Services.BackOffice.Application.Queries.ReportVipQueries;

public class
    GetVipReportQuery(IReportClient reportClient) : IGetService<GetVipReportReportRequest,
        GetVipReportReportResponse>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetVipReportReportResponse> GetAsync(
        GetVipReportReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetVipReportRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            Level = request.Level,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetVipReportAsync(reportRequest);

        if (!response.Valid)
        {
            return new GetVipReportReportResponse
            {
                Valid = response.Valid,
                Message = response.Message,
                TotalCount = response.TotalCount,
                Items = [],
                Total = new VipReportTotalModel(
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

        var pageArray = response.Items?.Select(x => new VipReportModel
        {
            BrandId = x.BrandId,
            Username = x.Username,
            PlayerCode = x.PlayerCode,
            Level = x.Level,
            DepositAmount = x.DepositAmount,
            WithdrawalAmount = x.WithdrawalAmount,
            BonusAmount = x.BonusAmount,
            CompanyAdjustAmount = x.CompanyAdjustAmount,
            BonusAdjustAmount = x.BonusAdjustAmount,
            RebateAmount = x.RebateAmount,
            BetAmount = x.BetAmount,
            ValidBetAmount = x.ValidBetAmount,
            Turnover = x.Turnover,
            WinLossAmount = x.WinLossAmount,
            NetProfit = x.NetProfit
        });

        var totalRequest = new GetVipReportTotalAmountRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            Level = request.Level,
            From = request.From,
            To = request.To
        };

        var totalResponse = await _reportClient.GetVipReportTotalAmountAsync(totalRequest);

        var total = new VipReportTotalModel(
            totalResponse.DepositAmount,
            totalResponse.WithdrawalAmount,
            totalResponse.CompanyAdjustAmount,
            totalResponse.BonusAdjustAmount,
            totalResponse.RebateAmount,
            totalResponse.BonusAmount,
            totalResponse.BetAmount,
            totalResponse.ValidBetAmount,
            totalResponse.Turnover,
            totalResponse.WinLossAmount,
            totalResponse.NetProfit
        );

        return new GetVipReportReportResponse
        {
            TotalCount = response.TotalCount,
            Items = pageArray.ToArray(),
            Total = total
        };
    }
}
