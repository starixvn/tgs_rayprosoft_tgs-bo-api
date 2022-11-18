using System.Collections.Generic;

namespace TGS.Services.BackOffice.Domain;

public class DepositAccumulateRouletteCollection
{
    public IEnumerable<DepositAccumulateRouletteItem> RouletteItems { get; init; } = default!;
}
