#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class GetPlayerBucketAmountQueryService : IGetService<GetPlayerBucketAmountQuery, ReadBucketResult>
{
    private readonly IPlayerBucketClient _playerBucketClient;

    public GetPlayerBucketAmountQueryService(IPlayerBucketClient playerBucketClient)
    {
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
    }

    public ValueTask<ReadBucketResult> GetAsync(
        GetPlayerBucketAmountQuery query,
        CancellationToken cancellationToken = default)
        => _playerBucketClient.ReadBucketsAsync(query.PlayerCode, cancellationToken);
}
