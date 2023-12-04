using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetBrandBucketTypesQueryService(IPlayerBucketClient playerBucketClient) : IQueryService<GetBrandBucketTypesQuery, string>
{
    private readonly IPlayerBucketClient _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));

    public IAsyncEnumerable<string> QueryAsync(GetBrandBucketTypesQuery query, CancellationToken cancellationToken = default)
        => _playerBucketClient.GetBrandBucketTypesAsync(query.BrandId, cancellationToken);
}
