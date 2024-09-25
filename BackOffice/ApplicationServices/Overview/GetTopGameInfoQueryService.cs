using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetTopGameInfoQueryService(
    IGameService gameService) : IQueryService<GetTopGameInfoQuery, TopGameInfo>
{
    private readonly IGameService _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));

    public IAsyncEnumerable<TopGameInfo> QueryAsync(GetTopGameInfoQuery query, CancellationToken cancellationToken = default)
    {
        var gameInfos = _gameService.GetGameQueries()
                .Where(g => query.GameProviderIds.Contains(g.GameProviderId))
                .Where(g => query.GameIds.Contains(g.Id))
                .Select(x => new
                {
                    GameId = x.Id,
                    GameName = x.Name,
                    x.Localization,
                    x.GameProviderId,
                    SubGameProviderName = x.SubGameProvider.Name,
                }).ToAsyncEnumerable();

        return gameInfos.Select(x =>
        {
            var localization = !string.IsNullOrWhiteSpace(x.Localization) ? x.Localization : "{}";

            var gameNameCultureDict = JsonSerializer.Deserialize<Dictionary<string, GameLocalization>>(localization)
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value.Name))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return new TopGameInfo
            {
                GameId = x.GameId,
                GameName = x.GameName,
                LocalizationDicts = gameNameCultureDict,
                GameProviderId = x.GameProviderId,
                SubGameProviderName = x.SubGameProviderName
            };
        });
    }
}
