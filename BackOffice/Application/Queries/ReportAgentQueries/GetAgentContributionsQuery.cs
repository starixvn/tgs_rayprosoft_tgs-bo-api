using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentQueries;

public class
    GetAgentContributionsQuery : IGetService<GetAgentContributionsReportRequest,
        GetAgentContributionsReportResponse>
{
    private readonly IReportClient _reportClient;

    public GetAgentContributionsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetAgentContributionsReportResponse> GetAsync(GetAgentContributionsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAgentContributionsRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            Username = request.Username,
            Type = (short?)request.Type,
            From = request.From,
            To = request.To,
            IncludeAgents = request.IncludeAgents,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = true
        };

        var response = await _reportClient.GetAgentContributionsAsync(reportRequest).ConfigureAwait(false);

        if (!response.Valid)
        {
            return new GetAgentContributionsReportResponse
            {
                Valid = response.Valid,
                Message = response.Message,
                TotalCount = response.TotalCount,
                Items = Array.Empty<AgentContributionReportModel>(),
                Total = new AgentContributionTotalModel(
                    DepositAmountSummary: 0,
                    DepositHandlingFeeSummary: 0,
                    WithdrawalAmountSummary: 0,
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

        var pageArray = response.Items?.Select(x => new AgentContributionReportModel
        {
            Username = x.Username,
            Type = (AccountType)x.Type,
            DepositAmount = x.DepositAmount,
            DepositHandlingFee = x.DepositHandlingFee,
            WithdrawalAmount = x.WithdrawalAmount,
            BonusAdjustAmount = x.BonusAdjustAmount,
            AgentAdjustAmount = x.AgentAdjustAmount,
            BonusAmount = x.BonusAmount,
            BetAmount = x.BetAmount,
            ValidBetAmount = x.ValidBetAmount,
            Turnover = x.Turnover,
            WinLossAmount = x.WinLossAmount,
            NetProfit = x.NetProfit
        }).AsQueryable().CreatePagedResponse(new PageableRequest
        {
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy
        });

        var total = new AgentContributionTotalModel(
            DepositAmountSummary: response.Items.Sum(x => x.DepositAmount),
            DepositHandlingFeeSummary: response.Items.Sum(x => x.DepositHandlingFee),
            WithdrawalAmountSummary: response.Items.Sum(x => x.WithdrawalAmount),
            BonusAdjustAmountSummary: response.Items.Sum(x => x.BonusAdjustAmount),
            AgentAdjustAmountSummary: response.Items.Sum(x => x.AgentAdjustAmount),
            BonusAmountSummary: response.Items.Sum(x => x.BonusAmount),
            BetAmountSummary: response.Items.Sum(x => x.BetAmount),
            ValidBetAmountSummary: response.Items.Sum(x => x.ValidBetAmount),
            TurnoverSummary: response.Items.Sum(x => x.Turnover),
            WinLossAmountSummary: response.Items.Sum(x => x.WinLossAmount),
            NetProfitSummary: response.Items.Sum(x => x.NetProfit));

        return new GetAgentContributionsReportResponse
        {
            TotalCount = response.TotalCount,
            Items = pageArray.Items,
            Total = total
        };
    }
}
