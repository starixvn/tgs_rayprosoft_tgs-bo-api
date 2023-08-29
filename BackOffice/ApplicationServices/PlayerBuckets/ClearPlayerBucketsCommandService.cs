#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

internal class ClearPlayerBucketsCommandService
    : ICommandService<ClearPlayerBucketsCommand>
    , ICommandService<ClearPlayerTypeBucketsCommand>
{
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerBucketForTypesClient _playerBucketForTypesClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public ClearPlayerBucketsCommandService(
        IPlayerServiceClient playerServiceClient,
        IPlayerBucketClient playerBucketClient,
        IPlayerBucketForTypesClient playerBucketForTypesClient)
    {
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _playerBucketForTypesClient = playerBucketForTypesClient ?? throw new ArgumentNullException(nameof(playerBucketForTypesClient));
    }

    public ValueTask ExecuteAsync(ClearPlayerBucketsCommand command, CancellationToken cancellationToken = default)
        => _playerBucketClient.CleanBucketsAsync(
            command.PlayerCode,
            command.AnchorId,
            null,
            null,
            cancellationToken);

    public async ValueTask ExecuteAsync(ClearPlayerTypeBucketsCommand command, CancellationToken cancellationToken = default)
    {
        var results = _playerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            command.PlayerCode,
            command.BucketTypes,
            cancellationToken);

        await _playerBucketForTypesClient.CleanBucketsForBucketTypesAsync(
            command.PlayerCode,
            results
                .Where(result => result.Result.Amount > 0
                    && result.Result.AnchorBucketId is not null)
                .Select(result => (
                    BucketType: result.BucketType?.Split(":").Last(),
                    AnchorBucketId: result.Result.AnchorBucketId!.Value))
                .ToEnumerable(),
            null,
            null,
            cancellationToken)
            .ConfigureAwait(false);
    }
}
