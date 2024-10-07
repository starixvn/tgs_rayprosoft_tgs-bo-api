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

public class
    GetPlayersAccountChangesQuery(IReportClient reportClient) : IGetService<GetPlayersAccountChangesReportRequest,
    GetPlayersAccountChangesReportResponse>
{
    private readonly IReportClient
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetPlayersAccountChangesReportResponse> GetAsync(
        GetPlayersAccountChangesReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetPlayersAccountChangesRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            TransactionType = (short?)request.TransactionType,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetMultiplePlayersAccountChangesAsync(reportRequest)
            .ToArrayAsync(cancellationToken);

        var reportTotalRequest = new GetPlayersAccountChangesTotalAmountRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            TransactionType = (short?)request.TransactionType,
            From = request.From,
            To = request.To
        };

        var totalResponse = await _reportClient.GetMultiplePlayersAccountChangesTotalAmountAsync(reportTotalRequest);

        return new GetPlayersAccountChangesReportResponse
        {
            Items = response.Select(x => new PlayersAccountChangeReportModel
            {
                Username = x.Username,
                Amount = x.Amount,
                BeforeBalance = x.BeforeBalance,
                AfterBalance = x.AfterBalance,
                TransactionType = (TransactionType)x.TransactionType,
                RecordedOn = x.RecordedOn,
                Operator = x.Operator,
                Description = x.Description
            }),
            Total = new PlayersAccountChangesTotalAmountReportModel
            {
                Amount = totalResponse.Amount,
                BeforeBalance = totalResponse.BeforeBalance,
                AfterBalance = totalResponse.AfterBalance
            },
            TotalCount = totalResponse.TotalCount
        };
    }
}
