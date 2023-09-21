using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                    .Select(x => new BrandGameInfo(
                        x.GameId,
                        x.Game.Name,
                        x.Game.Localization,
                        x.Game.GameProviderId,
                        x.Game.SubGameProvider.Name));

        return brandGameInfos.ToAsyncEnumerable();
    }
}
