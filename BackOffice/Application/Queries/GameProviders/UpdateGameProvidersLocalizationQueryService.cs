using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Json;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class UpdateGameProvidersLocalizationQueryService(
    ITgsRepository tgsRepository,
    IJsonSerializationProvider jsonSerializationProvider)
        : IQueryService<UpdateGameProvidersLocalizationCommand, string>
{
    private readonly IJsonSerializationProvider _jsonSerializationProvider = jsonSerializationProvider ?? throw new ArgumentNullException(nameof(jsonSerializationProvider));
    private readonly ITgsRepository _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));

    public async IAsyncEnumerable<string> QueryAsync(
        UpdateGameProvidersLocalizationCommand query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var subGp in query.SubGameProviders)
        {
            var game = await _tgsRepository.SubGameProviders
                .FirstOrDefaultAsync(x => x.Code == subGp.Code, cancellationToken)
                .ConfigureAwait(false);

            if (game == null)
            {
                yield return subGp.Code;
                continue;
            }

            game.Localization = _jsonSerializationProvider.SerializeToString(subGp.Localization);
            game.UpdatedByUsername = query.EditorUserName;
            game.UpdatedOn = DateTimeOffset.Now;
        }

        _ = await _tgsRepository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
