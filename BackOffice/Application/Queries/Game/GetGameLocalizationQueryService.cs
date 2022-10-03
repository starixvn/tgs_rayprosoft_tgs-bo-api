using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;
using Microsoft.EntityFrameworkCore;

namespace TGS.Services.BackOffice.Application.Queries.Game;

internal class GetGameLocalizationQueryService : IGetService<GetGameLocalizationQuery, GameLocalizationInfo>
{
    private readonly ITgsRepository _tgsRepository;

    public GetGameLocalizationQueryService(
        ITgsRepository tgsRepository)
        => _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));

    public async ValueTask<GameLocalizationInfo> GetAsync(
        GetGameLocalizationQuery query,
        CancellationToken cancellationToken = default)
    {
        var gameProvider =
            await _tgsRepository.GameProviders.FirstOrDefaultAsync(x => x.Code == query.GameProviderCode);

        var gpId = gameProvider.Id;

        var providerLanguages = await _tgsRepository.GameProviderLanguages
            .AsAsyncEnumerable()
            .Where(x => x.GameProviderId == gpId)
            .ToArrayAsync();

        var games = await _tgsRepository.Games
            .AsAsyncEnumerable()
            .Where(x => x.GameProviderId == gpId)
            .ToArrayAsync();

        return new GameLocalizationInfo
        {
            Languages = providerLanguages.Select(x => x.CultureCode),
            Games = games.Select(x =>
                new GameData
                {
                    GameId = x.Id,
                    GameCode = x.Code,
                    GameName = x.Name,
                    Localization = x.Localization
                })
        };
    }
}