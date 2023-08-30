#nullable enable

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
public record GetPlayerBucketsQuery(string PlayerCode, string? BucketType);
