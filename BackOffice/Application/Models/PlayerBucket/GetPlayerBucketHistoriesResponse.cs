using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerBucket;

public class GetPlayerBucketHistoriesResponse : MessageResponse
{
    public PlayerBucketHistoryModel[] Items { get; init; }
}
