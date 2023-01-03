using System;

namespace TGS.Services.BackOffice.Application.Models.PlayerBucket;

public record PlayerBucketHistoryModel
{
    public Guid PlayerBucketId { get; set; }

    public decimal Turnover { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
}
