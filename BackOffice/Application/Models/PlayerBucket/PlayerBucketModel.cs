using System;
using Tgs.Service.PlayerBuckets;

namespace TGS.Services.BackOffice.Application.Models.PlayerBucket;

public record PlayerBucketModel
{
    public Guid Id { get; set; }

    public string PlayerCode { get; set; }

    public decimal WageringRequirement { get; set; }

    public decimal CompletedWagering { get; set; }

    public decimal UncompleteWagering { get; set; }

    public BucketType BucketType { get; set; }

    public DateTime CreatedOn { get; set; }
}
