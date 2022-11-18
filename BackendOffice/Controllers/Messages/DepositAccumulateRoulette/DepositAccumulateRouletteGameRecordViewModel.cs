#nullable enable

using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.DepositAccumulateRoulette;

public class DepositAccumulateRouletteGameRecordViewModel
{
    public DateTimeOffset CreatedOn { get; init; }

    public string PlayerCode { get; init; } = default!;

    public Guid PromotionId { get; init; }

    public string PromotionName { get; init; } = default!;

    public Guid RecordId { get; init; }

    public string UserName { get; init; } = default!;
}
