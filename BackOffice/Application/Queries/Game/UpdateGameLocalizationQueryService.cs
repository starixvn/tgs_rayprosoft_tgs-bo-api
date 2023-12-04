using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Extensions;

namespace TGS.Services.BackOffice.Application.Queries.Game;

internal class UpdateGameLocalizationQueryService(
    ITgsRepository tgsRepository) : IGetService<UpdateGameLocalizationQuery, UpdateGameLocationInfo>
{
    private readonly ITgsRepository _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));

    public async ValueTask<UpdateGameLocationInfo> GetAsync(UpdateGameLocalizationQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryGameIds = query.Games.Select(x => x.Id).ToList();

        var existGameIds = await _tgsRepository.Games.AsAsyncEnumerable()
            .Where(p => queryGameIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToArrayAsync(cancellationToken);

        var diffGames = query.Games.Select(x => x.Id).ToArray().Except(existGameIds).ToArray();

        if (diffGames.Length == 0)
        {
            foreach (var item in existGameIds)
            {
                var model = query.Games.First(x => x.Id == item);
                var game = await _tgsRepository.Games.FirstOrDefaultAsync(x => x.Id == item, cancellationToken);
                game.Localization = model.Localization.ToJson();
                game.UpdatedByUsername = query.EditorUsername;
                game.UpdatedOn = DateTimeOffset.Now;
            }

            _ = await _tgsRepository.SaveChangesAsync(cancellationToken);

            return new UpdateGameLocationInfo
            {
                DiffGameIds = []
            };
        }

        return new UpdateGameLocationInfo { DiffGameIds = diffGames };
    }
}
