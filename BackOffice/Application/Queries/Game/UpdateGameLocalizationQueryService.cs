using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;

namespace TGS.Services.BackOffice.Application.Queries.Game;

internal class UpdateGameLocalizationQueryService(
    ITgsRepository tgsRepository) : IGetService<UpdateGameLocalizationQuery, UpdateGameLocationInfo>
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async ValueTask<UpdateGameLocationInfo> GetAsync(UpdateGameLocalizationQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryGameIds = query.Games.Select(x => x.Id).ToList();

        var existGameIds = await tgsRepository.Games.AsAsyncEnumerable()
            .Where(p => queryGameIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToArrayAsync(cancellationToken);

        var diffGames = query.Games.Select(x => x.Id).ToArray().Except(existGameIds).ToArray();

        if (diffGames.Length != 0)
            return new UpdateGameLocationInfo { DiffGameIds = diffGames };

        {
            foreach (var item in existGameIds)
            {
                var model = query.Games.First(x => x.Id == item);
                var game = await tgsRepository.Games.FirstOrDefaultAsync(x => x.Id == item, cancellationToken);

                game.Localization = JsonSerializer.Serialize(model.Localization, _options);
                game.UpdatedByUsername = query.EditorUsername;
                game.UpdatedOn = DateTimeOffset.Now;
            }

            _ = await tgsRepository.SaveChangesAsync(cancellationToken);

            return new UpdateGameLocationInfo { DiffGameIds = [] };
        }
    }
}
