using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;
public record OfflineDepositSettings(IEnumerable<string> Players, decimal BucketRate, OfflineDepositHierarchy Hierarchy);
