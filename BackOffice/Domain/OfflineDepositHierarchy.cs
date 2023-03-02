using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;
public record OfflineDepositHierarchy(IEnumerable<OfflineDepositPhase> Phases);
