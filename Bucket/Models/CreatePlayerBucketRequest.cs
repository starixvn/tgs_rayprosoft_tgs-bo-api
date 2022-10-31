using System;
using Tgs.Service.PlayerBuckets;

namespace TGS.Services.PlayerBucket.Models;

#nullable enable

public record CreatePlayerBucketRequest(
    Guid BrandId,
    string PlayerCode,
    Guid DefaultWalletId,
    BucketType BucketType,
    decimal BucketAmount,
    string? ExternalId);
