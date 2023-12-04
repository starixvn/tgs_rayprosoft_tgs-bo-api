using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;

internal class
    GetPlayerAllAgentRebatesQuery(IReportPlayerAllAgentClient reportClient) : IQueryService<GetPlayerAllAgentRebatesReportRequest,
        PlayerAllAgentRebateReportModel>
{
    private readonly IReportPlayerAllAgentClient _reportClient = reportClient;

    public IAsyncEnumerable<PlayerAllAgentRebateReportModel> QueryAsync(GetPlayerAllAgentRebatesReportRequest request,
        CancellationToken cancellationToken)
    {
        var reportRequest = new GetPlayerAllAgentRebatesRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        return _reportClient
            .GetPlayerAllAgentRebatesAsync(reportRequest)
            .Select(x => new PlayerAllAgentRebateReportModel
            {
                Username = x.Username,
                PlayerCode = x.PlayerCode,
                MaxActivePlayerNumber = x.MaxActivePlayerNumber,
                ValidBet = x.ValidBet,
                Rebate = x.Rebate,
                SettleDateTime = x.SettleDateTime,
                SentOn = x.SentOn,
                From = x.From,
                To = x.To
            });
    }
}