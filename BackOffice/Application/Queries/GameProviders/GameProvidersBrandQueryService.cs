using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class GameProvidersBrandQueryService
    : IGetService<GameProvidersBrandQueryQuery, GameProvidersBrandQueryResult>
{
    private readonly ITgsRepository _tgsRepository;

    public GameProvidersBrandQueryService(ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public ValueTask<GameProvidersBrandQueryResult> GetAsync(GameProvidersBrandQueryQuery query, CancellationToken cancellationToken = default)
    {
        var gameProviders = _tgsRepository.GameProviders
            .Where(query.GameProviderId.HasValue, x => x.Id == query.GameProviderId.Value)
            .OrderBy(x => x.Code)
            .Select(x => new GameProvidersBrandQueryGameProviderData(
                x.Id,
                x.Code,
                x.Name
                )
            ).ToArray();

        var brands = _tgsRepository.Brands
            .Where(query.Brand.HasValue, x => x.Id == query.Brand.Value)
            .Where(x => x.Status != BrandStatus.Archived)
            .OrderBy(x => x.Code)
            .Select(x => new GameProvidersBrandQueryBrandData(x.Id, x.Name, x.Code))
            .ToArray();

        foreach (var brand in brands)
        {
            brand.GameProvider = gameProviders.GroupJoin(
                _tgsRepository.BrandGameProviders.AsQueryable().Where(brandGameProviders => brandGameProviders.BrandId == brand.BrandId).ToArray(),
                gameProvider => gameProvider.GameProviderId,
                brandGameProvider => brandGameProvider.GameProviderId,
                (gameProvider, brandGameProvider) => new
                {
                    gameProviderCode = gameProvider.GameProviderCode,
                    brandGameProvider
                })
                .OrderBy(x => x.gameProviderCode)
                .SelectMany(c => c.brandGameProvider.DefaultIfEmpty(), (gameProvider, brandGameProvider)
                => new
                {
                    GameProviderCode = gameProvider.gameProviderCode,
                    Exist = brandGameProvider != null && brandGameProvider.Status != Status.Inactive
                }).ToDictionary(x => x.GameProviderCode, x => x.Exist);
        }

        return ValueTask.FromResult<GameProvidersBrandQueryResult>(new()
        {
            GameProviders = gameProviders,
            GameProvidersBrands = brands
        });
    }
}
