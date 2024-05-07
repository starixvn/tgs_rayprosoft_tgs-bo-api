using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportAgent;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentQueries;

public class
    GetAgentDividendsProductFeeQuery(IReportClient reportClient) : IGetService<GetAgentDividendProductFeeReportRequest,
        GetAgentDividendProductFeeReportResponse>
{
    private readonly IReportClient
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetAgentDividendProductFeeReportResponse> GetAsync(
        GetAgentDividendProductFeeReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAgentDividendProductFeeRequest
        {
            AgentId = request.AgentId,
            AgentDividendPercentage = request.AgentDividendPercentage,
            From = request.From,
            To = request.To
        };

        var response = await _reportClient.GetAgentDividendProductFeeAsync(reportRequest);

        return new GetAgentDividendProductFeeReportResponse
        {
            Valid = true,
            LiveDealerAmount = response.LiveDealerAmount,
            SlotAmount = response.SlotAmount,
            LotteryAmount = response.LotteryAmount,
            CardGameAmount = response.CardGameAmount,
            SportsAmount = response.SportsAmount,
            VirtualSportsAmount = response.VirtualSportsAmount,
        };
    }
}
