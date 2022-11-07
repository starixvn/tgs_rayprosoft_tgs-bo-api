using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.PlayerBucket;

namespace TGS.Services.BackOffice.Application.Commands.PlayerHandlers;

public class ClearPlayerBucketsHandler : Handler<ClearPlayerBucketsRequest, ClearPlayerBucketsResponse>
{
    private readonly IPlayerBucketRepository _playerBucketRepository;
    private readonly IPlayerService _service;

    public ClearPlayerBucketsHandler(IPlayerBucketRepository playerBucketRepository, IPlayerService service,
        IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
        _playerBucketRepository = playerBucketRepository;
    }

    public override async Task<ClearPlayerBucketsResponse> Handle(
        ClearPlayerBucketsRequest request,
        CancellationToken cancellationToken)
    {
        var player = await _service.GetPlayerAsync(x => x.PlayerCode == request.PlayerCode).ConfigureAwait(false);

        if (player is null)
        {
            return CreateInvalidResponse<ClearPlayerBucketsResponse>(Player.NotFound);
        }

        var bucketInfo = await _playerBucketRepository.GetPlayerBucketInfoAsync(
            player.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (bucketInfo.Amount > 0M && bucketInfo.SpanshotId is not null)
            await _playerBucketRepository.ClearPlayerBucketsAsync(
                player.PlayerCode,
                bucketInfo.SpanshotId.Value,
                null,
                cancellationToken).ConfigureAwait(false);

        return new ClearPlayerBucketsResponse
        {
            Valid = true,
            wageringRequirement = bucketInfo.Amount
        };
    }
}
