using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetAllGamesByGameProviderIdQueryService : IQueryService<GetAllGamesByGameProviderIdQuery, GameInfo>
{
    private readonly IGameRepository _gameRepository;

    public GetAllGamesByGameProviderIdQueryService(
        IGameRepository backOfficeRepository)
    {
        _gameRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public IAsyncEnumerable<GameInfo> QueryAsync(
        GetAllGamesByGameProviderIdQuery query,
        CancellationToken cancellationToken = default)
        => _gameRepository.QueryAllGamesAsync(query.GameProviderId, cancellationToken);
}
