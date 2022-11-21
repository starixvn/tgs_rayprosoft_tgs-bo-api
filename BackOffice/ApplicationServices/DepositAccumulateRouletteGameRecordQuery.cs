#nullable enable

using System;
using System.Collections.Generic;

namespace TGS.Services.BackOffice.ApplicationServices;

public record DepositAccumulateRouletteGameRecordQuery(
    Guid BrandId,
    IEnumerable<Guid> PromotionIds,
    IEnumerable<string> PlayerCodes,
    (DateTimeOffset Begin, DateTimeOffset End) TimeRange);
