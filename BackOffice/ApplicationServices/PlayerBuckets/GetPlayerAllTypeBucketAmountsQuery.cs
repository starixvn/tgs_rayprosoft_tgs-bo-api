using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
public record GetPlayerAllTypeBucketAmountsQuery(string PlayerCode, IEnumerable<string> BucketTypes);
