using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record DepositAccumulateRouletteRecordDetailQuery(
    Guid BrandId,
    Guid RecordId);
