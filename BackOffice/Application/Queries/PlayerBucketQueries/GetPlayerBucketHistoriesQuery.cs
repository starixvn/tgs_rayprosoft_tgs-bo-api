using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TGS.Services.BackOffice.Application.Models.PlayerBucket;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PlayerBucketQueries;

public class
    GetPlayerBucketHistoriesQuery : IGetService<GetPlayerBucketHistoriesRequest, GetPlayerBucketHistoriesResponse>
{
    private readonly IPlayerBucketService _playerBucketService;

    public GetPlayerBucketHistoriesQuery(IPlayerBucketService playerBucketService)
        => _playerBucketService =
            playerBucketService ?? throw new ArgumentNullException(nameof(playerBucketService));

    public async ValueTask<GetPlayerBucketHistoriesResponse> GetAsync(GetPlayerBucketHistoriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var playerBucketHistories =
            await _playerBucketService.GetPlayerBucketHistoriesAsync(request.PlayerBucketId);

        return new GetPlayerBucketHistoriesResponse
        {
            Valid = true,
            Items = playerBucketHistories.Select(x => new PlayerBucketHistoryModel
            {
                PlayerBucketId = x.PlayerBucketId,
                PlayerCode = x.PlayerCode,
                Turnover = x.Turnover,
                Data = x.RoundData,
                CreatedOn = x.CreatedOn
            }).ToArray()
        };
    }
}