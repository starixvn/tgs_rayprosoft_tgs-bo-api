using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs;
using TGS.Core.Infrastructures.Json;

namespace TGS.Services.BackOffice.Application.Queries.GameProviders;

internal class UpdateGameProvidersLocalizationQueryService
    : IQueryService<UpdateGameProvidersLocalizationCommand, string>
{
    private readonly IJsonSerializationProvider _jsonSerializationProvider;
    private readonly ITgsRepository _tgsRepository;

    public UpdateGameProvidersLocalizationQueryService(
        ITgsRepository tgsRepository,
        IJsonSerializationProvider jsonSerializationProvider)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _jsonSerializationProvider = jsonSerializationProvider ?? throw new ArgumentNullException(nameof(jsonSerializationProvider));
    }

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

        await _tgsRepository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
