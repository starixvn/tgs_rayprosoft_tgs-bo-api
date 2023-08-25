using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetBrandBucketTypesQueryService : IQueryService<GetBrandBucketTypesQuery, string>
{
    private readonly IPlayerBucketClient _playerBucketClient;

    public GetBrandBucketTypesQueryService(IPlayerBucketClient playerBucketClient)
    {
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public IAsyncEnumerable<string> QueryAsync(GetBrandBucketTypesQuery query, CancellationToken cancellationToken = default)
        => _playerBucketClient.GetBrandBucketTypesAsync(query.BrandId, cancellationToken);
}
