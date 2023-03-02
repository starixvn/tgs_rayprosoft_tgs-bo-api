using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;
public record OfflineLoginSettings(
    decimal BonusAmount,
    decimal TurnoverMultiple,
    IEnumerable<string> PlayerCodes);
