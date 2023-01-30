using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class GameRepository : IGameRepository
{
    private readonly ITgsRepository _tgsRepository;

    public GameRepository(
        ITgsRepository tgsRepository)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
    }

    public async IAsyncEnumerable<GameInfo> QueryAllGamesAsync(
        Guid gameProviderId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var games = await _tgsRepository.Games
            .Where(g => g.GameProviderId == gameProviderId)
            .Select(g => new GameInfo(
                g.Name,
                g.Code,
                g.RuleUrl,
                g.ExternalId,
                g.Type,
                g.PlatformType,
                g.HasTurnoverOnTieBet,
                g.EndpointPath,
                g.Description,
                g.SubGame,
                g.AvailableToBrand,
                g.Html5,
                g.AvailableCurrencies,
                g.SubGameProviderCode))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var game in games)
            yield return game;
    }
}
