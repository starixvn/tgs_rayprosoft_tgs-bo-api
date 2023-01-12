using System;
using System.Collections.Generic;
using System.Threading;

namespace TGS.Services.BackOffice.Domain;

public interface IGameRepository
{
    IAsyncEnumerable<GameInfo> QueryAllGamesAsync(
        Guid gameProviderId,
        CancellationToken cancellationToken = default);
}