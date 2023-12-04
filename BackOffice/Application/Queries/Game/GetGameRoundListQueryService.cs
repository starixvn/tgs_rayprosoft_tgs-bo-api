using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Game;

internal class GetGameRoundListQueryService(IReportClient reportClient) : IGetService<GetGameRoundListQuery, PagedResponse<RoundModel>>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<RoundModel>> GetAsync(
        GetGameRoundListQuery query,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetRoundsRequest
        {
            BrandId = query.BrandId,
            Username = query.Username,
            PlayerCode = query.PlayerCode,
            RoundId = query.RoundId,
            ExternalRoundId = query.ExternalRoundId,
            GameProviderId = query.GameProviderId,
            Status = (short?)query.Status,
            GameType = (short?)query.GameType,
            From = query.From,
            To = query.To,
            ClosedFrom = query.ClosedFrom,
            ClosedTo = query.ClosedTo,
            Page = query.Paging.Page,
            Rows = query.Paging.Rows,
            OrderBy = (short)query.Paging.OrderBy,
            SortBy = query.Paging.SortBy,
            ShowAll = query.Paging.ShowAll
        };

        return await _reportClient
            .GetRoundsAsync(reportRequest)
            .ConfigureAwait(false);
    }
}
