#nullable enable

using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.DepositAccumulateRoulette;

public class DepositAccumulateRouletteGameRecordsRequest
{
    public DateTimeOffset? From { get; init; }

    public string? PromotionName { get; init; }

    public DateTimeOffset? To { get; init; }

    public string? UserName { get; init; }
}
