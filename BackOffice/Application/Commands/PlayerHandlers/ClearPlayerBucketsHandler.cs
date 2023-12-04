using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;

namespace TGS.Services.BackOffice.Application.Commands.PlayerHandlers;

public class ClearPlayerBucketsHandler : Handler<ClearPlayerBucketsRequest, ClearPlayerBucketsResponse>
{
    private readonly IPlayerBucketClient _playerBucketClient;
    private readonly IPlayerServiceClient _playerServiceClient;

    public ClearPlayerBucketsHandler(
        IPlayerBucketClient playerBucketClient,
        IPlayerServiceClient playerServiceClient,
        IHttpContextAccessor accessor)
        : base(accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        _playerBucketClient = playerBucketClient ?? throw new ArgumentNullException(nameof(playerBucketClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
    }

    public override async Task<ClearPlayerBucketsResponse> Handle(
        ClearPlayerBucketsRequest request,
        CancellationToken cancellationToken)
    {
        var player = await _playerServiceClient.GetPlayerInfoAsync(
            request.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (player is null)
            return CreateInvalidResponse<ClearPlayerBucketsResponse>(Player.NotFound);

        var bucketResult = await _playerBucketClient.ReadBucketsAsync(
            player.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (bucketResult.Amount > 0M && bucketResult.AnchorBucketId is not null)
            await _playerBucketClient.CleanBucketsAsync(
                player.PlayerCode,
                bucketResult.AnchorBucketId.Value,
                null,
                null,
                cancellationToken).ConfigureAwait(false);

        return new ClearPlayerBucketsResponse
        {
            Valid = true,
            WageringRequirement = bucketResult.Amount
        };
    }
}
