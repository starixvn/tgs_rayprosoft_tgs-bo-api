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
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetTopTenGameQueryService : IQueryService<GetTopTenGameQuery, TopTenGameInfo>
{
    private readonly IBrandGameService _brandGameService;
    private readonly IBrandServiceClient _brandServiceClient;
    private readonly IGameWagersClient _gameWagersClient;

    public GetTopTenGameQueryService(
        IBrandServiceClient brandServiceClient,
        IGameWagersClient gameWagersClient,
        IBrandGameService brandGameService)
    {
        _brandServiceClient = brandServiceClient ?? throw new ArgumentNullException(nameof(brandServiceClient));
        _gameWagersClient = gameWagersClient ?? throw new ArgumentNullException(nameof(gameWagersClient));
        _brandGameService = brandGameService ?? throw new ArgumentNullException(nameof(brandGameService));
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
                        brandInfo.Code,
                        currentDay,
                        now,
                        BrandWagerGameMinsStatsSort.Bet,
                        false,
                        cancellationToken: cancellationToken)
                    .ToArrayAsync(cancellationToken);

                var gameProviderIds = wagerInfos.Select(x => x.GameProviderId).Distinct().ToArray();
                var gameIds = wagerInfos.Select(x => x.GameId).ToArray();

                var brandGameInfos = _brandGameService.GetBrandGameQueries()
                    .Where(x => x.BrandId == query.BrandId
                        && gameProviderIds.Contains(x.Game.GameProviderId)
                        && gameIds.Contains(x.GameId))
                    .Select(x => new
                    {
                        x.GameId,
                        GameName = x.Game.Name,
                        x.Game.GameProviderId,
                        GameProviderName = x.Game.SubGameProvider.Name
                    });

                foreach (var item in wagerInfos)
                {
                    var brandGame = brandGameInfos
                        .Where(x => x.GameProviderId == item.GameProviderId
                            && x.GameId == item.GameId)
                        .First();
                    yield return new TopTenGameInfo
                    {
                        GameName = brandGame.GameName,
                        GameProviderName = brandGame.GameProviderName,
                        Bet = item.Bet,
                        WinLoss = item.WinLoss,
                    };
                }
                break;

            case TopTenGameSortType.WinLoss:
                wagerInfos = await _gameWagersClient
                   .BrandWagerGameStatsQueryAsync(
                       brandInfo.Code,
                       currentDay,
                       now,
                       BrandWagerGameMinsStatsSort.WinLoss,
                       query.Ascending,
                       cancellationToken: cancellationToken)
                   .ToArrayAsync(cancellationToken);

                gameProviderIds = wagerInfos.Select(x => x.GameProviderId).Distinct().ToArray();
                gameIds = wagerInfos.Select(x => x.GameId).ToArray();

                brandGameInfos = _brandGameService.GetBrandGameQueries()
                    .Where(x => x.BrandId == query.BrandId
                        && gameProviderIds.Contains(x.Game.GameProviderId)
                        && gameIds.Contains(x.GameId))
                    .Select(x => new
                    {
                        x.GameId,
                        GameName = x.Game.Name,
                        x.Game.GameProviderId,
                        GameProviderName = x.Game.SubGameProvider.Name
                    });

                foreach (var item in wagerInfos)
                {
                    var brandGame = brandGameInfos
                       .Where(x => x.GameProviderId == item.GameProviderId
                           && x.GameId == item.GameId)
                       .First();
                    yield return new TopTenGameInfo
                    {
                        GameName = brandGame.GameName,
                        GameProviderName = brandGame.GameProviderName,
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
