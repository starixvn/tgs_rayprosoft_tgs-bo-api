using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

internal class GetBrandGameQueryService : IQueryService<GetBrandGameQuery, BrandGameInfo>
{
    private readonly IBrandGameService _brandGameService;

    public GetBrandGameQueryService(IBrandGameService brandGameService)
    {
        _brandGameService = brandGameService ?? throw new ArgumentNullException(nameof(brandGameService));
    }

    public IAsyncEnumerable<BrandGameInfo> QueryAsync(GetBrandGameQuery query, CancellationToken cancellationToken = default)
    {
        var brandGameInfos = _brandGameService.GetBrandGameQueries()
            .Where(x => x.BrandId == query.BrandId
                && query.GameIds.Contains(x.GameId))
            .AsNoTracking()
            .Select(x => new
            {
                x.GameId,
                GameName = x.Game.Name,
                x.Game.Localization,
                x.Game.GameProviderId,
                GameProviderName = x.Game.SubGameProvider.Name
            }).ToAsyncEnumerable();

        return brandGameInfos.Select(x =>
        {
            var localization = !string.IsNullOrWhiteSpace(x.Localization) ? x.Localization : "{}";

            var gameNameCultureDict = JsonSerializer.Deserialize<Dictionary<string, GameLocalization>>(localization)
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value.Name))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return new BrandGameInfo
            {
                GameId = x.GameId,
                GameName = x.GameName,
                LocalizationDicts = gameNameCultureDict,
                GameProviderId = x.GameProviderId,
                GameProviderName = x.GameProviderName
            };
        });
    }
}
