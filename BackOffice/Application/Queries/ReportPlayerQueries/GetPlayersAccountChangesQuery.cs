using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportPlayer;

namespace TGS.Services.BackOffice.Application.Queries.ReportPlayerQueries;

public class
    GetPlayersAccountChangesQuery : IGetService<GetPlayersAccountChangesReportRequest,
        PagedResponse<PlayersAccountChangeReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetPlayersAccountChangesQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<PlayersAccountChangeReportModel>> GetAsync(
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

        var response = await _reportClient.GetPlayersAccountChangesAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new PlayersAccountChangeReportModel
            {
                Username = x.Username,
                Amount = x.Amount,
                BeforeBalance = x.BeforeBalance,
                AfterBalance = x.AfterBalance,
                TransactionType = (TransactionType)x.TransactionType,
                RecordedOn = x.RecordedOn,
                Operator = x.Operator,
                Description = x.Description
            }).ToArray()
        };
    }
}
