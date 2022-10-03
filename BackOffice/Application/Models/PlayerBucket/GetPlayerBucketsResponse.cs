using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.PlayerBucket;

public class GetPlayerBucketsResponse : MessageResponse
{
    public PlayerBucketModel[] Items { get; init; }
}