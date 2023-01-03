using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.Game;

internal class GetSystemGameRoundListQueryService : IGetService<GetSystemGameRoundListQuery, PagedResponse<SystemRoundModel>>
{
    private readonly IReportClient _reportClient;

    public GetSystemGameRoundListQueryService(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<SystemRoundModel>> GetAsync(
        GetSystemGameRoundListQuery query,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetSystemRoundsRequest
        {
            Username = query.Username,
            PlayerCode = query.PlayerCode,
            ExternalRoundId = query.ExternalRoundId,
            RoundId = query.RoundId,
            ExternalFrom = query.ExternalFrom,
            ExternalTo = query.ExternalTo,
            ExternalClosedFrom = query.ExternalClosedFrom,
            ExternalClosedTo = query.ExternalClosedTo,
            From = query.From,
            To = query.To,
            ClosedFrom = query.ClosedFrom,
            ClosedTo = query.ClosedTo,
            Status = (short?)query.Status,
            BrandId = query.BrandId,
            GameProviderId = query.GameProviderId,
            SubGameProviderCode = query.SubGameProviderCode,
            GameType = (short?)query.GameType,
            CurrencyCode = query.CurrencyCode,
            Page = query.Paging.Page,
            Rows = query.Paging.Rows,
            OrderBy = (short)query.Paging.OrderBy,
            SortBy = query.Paging.SortBy,
            ShowAll = query.Paging.ShowAll
        };

        return await _reportClient
            .GetSystemRoundsAsync(reportRequest)
            .ConfigureAwait(false);
    }
}
