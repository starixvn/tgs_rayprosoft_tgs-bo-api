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

        var response = await _reportClient.GetAgentContributionsAsync(reportRequest);

        if (!response.Valid)
        {
            return new GetAgentContributionsReportResponse
            {
                Valid = response.Valid,
                Message = response.Message,
                TotalCount = response.TotalCount,
                Items = Array.Empty<AgentContributionReportModel>(),
                Total = new AgentContributionTotalModel(
                    DepositAmount: 0,
                    WithdrawalAmount: 0,
                    CompanyAdjustAmount: 0,
                    BonusAdjustAmount: 0,
                    AgentAdjustAmount: 0,
                    BonusAmount: 0,
                    BetAmount: 0,
                    ValidBetAmount: 0,
                    Turnover: 0,
                    WinLossAmount: 0,
                    NetProfit: 0,
                    DepositHandlingFee: 0)
            };
        }

        var pageArray = response.Items?.Select(x => new AgentContributionReportModel
        {
            Username = x.Username,
            Type = (AccountType)x.Type,
            DepositAmount = x.DepositAmount,
            DepositHandlingFee = 0m,
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
        }).AsQueryable().CreatePagedResponse(new PageableRequest
        {
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy
        });

        var total = new AgentContributionTotalModel(
            DepositAmount: response.Items.Sum(x => x.DepositAmount),
            WithdrawalAmount: response.Items.Sum(x => x.WithdrawalAmount),
            CompanyAdjustAmount: response.Items.Sum(x => x.CompanyAdjustAmount),
            BonusAdjustAmount: response.Items.Sum(x => x.BonusAdjustAmount),
            AgentAdjustAmount: response.Items.Sum(x => x.AgentAdjustAmount),
            BonusAmount: response.Items.Sum(x => x.BonusAmount),
            BetAmount: response.Items.Sum(x => x.BetAmount),
            ValidBetAmount: response.Items.Sum(x => x.ValidBetAmount),
            Turnover: response.Items.Sum(x => x.Turnover),
            WinLossAmount: response.Items.Sum(x => x.WinLossAmount),
            NetProfit: response.Items.Sum(x => x.NetProfit),
            DepositHandlingFee: 0
        );

        return new GetAgentContributionsReportResponse
        {
            TotalCount = response.TotalCount,
            Items = pageArray.Items,
            Total = total
        };
    }
}
