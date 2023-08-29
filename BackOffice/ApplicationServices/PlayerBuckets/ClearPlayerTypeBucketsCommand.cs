#nullable enable

using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;

public record ClearPlayerTypeBucketsCommand(string PlayerCode, IEnumerable<string> BucketTypes);
