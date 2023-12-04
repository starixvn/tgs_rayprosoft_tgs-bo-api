using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.GameAction;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportGameAction;

namespace TGS.Services.BackOffice.Application.Queries.ReportGameActionQueries;

public class
    GetGameActionsQuery(IReportClient reportClient) : IGetService<GetGameActionsReportRequest,
        PagedResponse<GameActionReportModel>>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<GameActionReportModel>> GetAsync(GetGameActionsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetGameActionsRequest
        {
            RoundId = request.RoundId,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll
        };

        var response = await _reportClient.GetGameActionsAsync(reportRequest);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new GameActionReportModel
            {
                RecordedOn = x.RecordedOn,
                ExternalTransactionId = x.ExternalTransactionId,
                GameActionType = (GameActionType)x.GameActionType,
                Amount = x.Amount,
                BeforeBalance = x.BeforeBalance,
                AfterBalance = x.AfterBalance
            }).ToArray()
        };
    }
}
