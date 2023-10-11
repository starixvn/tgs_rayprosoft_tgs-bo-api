using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;

internal class
    GetPlayerAllAgentRebateDetailGameTypesQuery : IQueryService<GetPlayerAllAgentRebateDetailGameTypesReportRequest,
        PlayerAllAgentRebateDetailGameTypeReportModel>
{
    private readonly IReportPlayerAllAgentClient _reportClient;

    public GetPlayerAllAgentRebateDetailGameTypesQuery(IReportPlayerAllAgentClient reportClient) =>
        _reportClient = reportClient;

    public IAsyncEnumerable<PlayerAllAgentRebateDetailGameTypeReportModel> QueryAsync(
        GetPlayerAllAgentRebateDetailGameTypesReportRequest request,
        CancellationToken cancellationToken)
    {
        var reportRequest = new GetPlayerAllAgentRebateDetailGameTypesRequest
        {
            BrandId = request.BrandId, PlayerCode = request.PlayerCode, SettleDateTime = request.SettleDateTime
        };

        return _reportClient
            .GetPlayerAllAgentRebateDetailGameTypesAsync(reportRequest)
            .Select(x => new PlayerAllAgentRebateDetailGameTypeReportModel
            {
                AllAgentGameType = x.AllAgentGameType,
                ActivePlayerNumber = x.ActivePlayerNumber,
                RebatePercentage = x.RebatePercentage
            });
    }
}