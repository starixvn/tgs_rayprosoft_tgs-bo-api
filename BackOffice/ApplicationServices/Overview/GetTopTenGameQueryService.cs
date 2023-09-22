#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.GameWagers.Client;
using Tgs.GameWagers.Common;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetTopTenGameQueryService : IQueryService<GetTopTenGameQuery, TopTenGameInfo>
{
    private readonly IBrandServiceClient _brandServiceClient;
    private readonly IGameWagersClient _gameWagersClient;

    public GetTopTenGameQueryService(
        IBrandServiceClient brandServiceClient,
        IGameWagersClient gameWagersClient)
    {
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
        _gameWagersClient = gameWagersClient ?? throw new ArgumentNullException(nameof(gameWagersClient));
    }

    public async IAsyncEnumerable<TopTenGameInfo> QueryAsync(
        GetTopTenGameQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8D));
        var currentDay = new DateTimeOffset(now.Date, TimeSpan.FromHours(8D)).UtcDateTime;

        var brandInfo = await _brandServiceClient
            .GetBrandInfoAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        switch (query.SortType)
        {
            case TopTenGameSortType.Bet:
                var wagerInfos = await _gameWagersClient
                    .BrandWagerGameStatsQueryAsync(
                        brandInfo!.Code,
                        startOn: currentDay,
                        endOn: now,
                        BrandWagerGameMinsStatsSort.Bet,
                        isAscending: false,
                        page: 1,
                        pageSize: 10,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                foreach (var item in wagerInfos)
                {
                    yield return new TopTenGameInfo
                    {
                        GameId = item.GameId,
                        GameProviderId = item.GameProviderId,
                        Bet = item.Bet,
                        WinLoss = item.WinLoss,
                    };
                }

                break;

            case TopTenGameSortType.WinLoss:
                wagerInfos = await _gameWagersClient
                   .BrandWagerGameStatsQueryAsync(
                       brandInfo!.Code,
                       startOn: currentDay,
                       endOn: now,
                       BrandWagerGameMinsStatsSort.WinLoss,
                       isAscending: query.Ascending,
                       page: 1,
                       pageSize: 10,
                       cancellationToken: cancellationToken)
                   .ToArrayAsync(cancellationToken)
                   .ConfigureAwait(false);

                foreach (var item in wagerInfos)
                {
                    yield return new TopTenGameInfo
                    {
                        GameId = item.GameId,
                        GameProviderId = item.GameProviderId,
                        Bet = item.Bet,
                        WinLoss = item.WinLoss,
                    };
                }

                break;

            default:
                break;
        }
    }
}
