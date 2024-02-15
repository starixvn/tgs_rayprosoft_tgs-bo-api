using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetAllGamesByBrandIdQueryService(IBackOfficeRepository backOfficeRepository)
    : IQueryService<GetAllGamesByBrandIdQuery, GameData>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public IAsyncEnumerable<GameData> QueryAsync(
        GetAllGamesByBrandIdQuery query,
        CancellationToken cancellationToken = default)
        => _backOfficeRepository.QueryAllGamesAsync(query.BrandId);
}
