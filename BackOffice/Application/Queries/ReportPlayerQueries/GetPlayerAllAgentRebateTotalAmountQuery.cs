using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;

internal class
    GetPlayerAllAgentRebateTotalAmountQuery(IReportPlayerAllAgentClient reportClient) : IGetService<GetPlayerAllAgentRebateTotalAmountReportRequest,
        PlayerAllAgentRebateTotalAmountReportModel>
{
    private readonly IReportPlayerAllAgentClient _reportClient = reportClient;

    public async ValueTask<PlayerAllAgentRebateTotalAmountReportModel> GetAsync(
        GetPlayerAllAgentRebateTotalAmountReportRequest request,
        CancellationToken cancellationToken)
    {
        var reportRequest = new GetPlayerAllAgentRebateTotalAmountRequest
        {
            BrandId = request.BrandId, Username = request.Username, From = request.From, To = request.To
        };

        var total = await _reportClient
            .GetPlayerAllAgentRebatesTotalAmountAsync(reportRequest);

        return new PlayerAllAgentRebateTotalAmountReportModel
        {
            TotalCount = total.TotalCount, ValidBet = total.ValidBet, Rebate = total.Rebate
        };
    }
}