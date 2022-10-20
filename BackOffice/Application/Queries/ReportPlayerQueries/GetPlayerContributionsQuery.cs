using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;

namespace TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;

public class GetPlayerContributionsQuery
    : IGetService<GetPlayerContributionsReportRequest, GetPlayerContributionsReportResponse>
{
    private readonly IReportClient _reportClient;

    public GetPlayerContributionsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetPlayerContributionsReportResponse> GetAsync(
        GetPlayerContributionsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetPlayerContributionsRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            Level = request.Level,
            HasUpperAgent = request.HasUpperAgent,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetPlayerContributionsAsync(reportRequest);

        if (!response.Valid)
        {
            return new GetPlayerContributionsReportResponse
            {
                Valid = response.Valid,
                Message = response.Message,
                TotalCount = response.TotalCount,
                Items = Array.Empty<PlayerContributionReportModel>(),
                Total = new PlayerContributionReportTotalModel(
                    DepositAmountSummary: 0,
                    DepositHandlingFeeSummary: 0,
                    WithdrawalAmountSummary: 0,
                    CompanyAdjustAmountSummary: 0,
                    BonusAdjustAmountSummary: 0,
                    AgentAdjustAmountSummary: 0,
                    BonusAmountSummary: 0,
                    BetAmountSummary: 0,
                    ValidBetAmountSummary: 0,
                    TurnoverSummary: 0,
                    WinLossAmountSummary: 0,
                    NetProfitSummary: 0)
            };
        }

        var pageArray = response.Items?.Select(x => new PlayerContributionReportModel
        {
            Username = x.Username,
            PlayerCode = x.PlayerCode,
            Type = (AccountType)x.Type,
            Level = x.Level,
            DepositAmount = x.DepositAmount,
            DepositHandlingFee = x.DepositHandlingFee,
            WithdrawalAmount = x.WithdrawalAmount,
            CompanyAdjustAmount = x.CompanyAdjustAmount,
            BonusAdjustAmount = x.BonusAdjustAmount,
            AgentAdjustAmount = x.AgentAdjustAmount,
            BonusAmount = x.BonusAmount,
            BetAmount = x.BetAmount,
            ValidBetAmount = x.ValidBetAmount,
            Turnover = x.Turnover,
            WinLossAmount = x.WinLossAmount,
            NetProfit = x.NetProfit
        });

        var totalRequest = new GetPlayerContributionsTotalAmountRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            Level = request.Level,
            HasUpperAgent = request.HasUpperAgent,
            From = request.From,
            To = request.To
        };

        var totalResponse = await _reportClient.GetPlayerContributionsTotalAmountAsync(totalRequest);

        var total = new PlayerContributionReportTotalModel(
            DepositAmountSummary: totalResponse.DepositAmount,
            DepositHandlingFeeSummary: totalResponse.DepositHandlingFee,
            WithdrawalAmountSummary: totalResponse.WithdrawalAmount,
            CompanyAdjustAmountSummary: totalResponse.CompanyAdjustAmount,
            BonusAdjustAmountSummary: totalResponse.BonusAdjustAmount,
            AgentAdjustAmountSummary: totalResponse.AgentAdjustAmount,
            BonusAmountSummary: totalResponse.BonusAmount,
            BetAmountSummary: totalResponse.BetAmount,
            ValidBetAmountSummary: totalResponse.ValidBetAmount,
            TurnoverSummary: totalResponse.Turnover,
            WinLossAmountSummary: totalResponse.WinLossAmount,
            NetProfitSummary: totalResponse.NetProfit);

        return new GetPlayerContributionsReportResponse
        {
            TotalCount = response.TotalCount,
            Items = pageArray.ToArray(),
            Total = total
        };
    }
}
