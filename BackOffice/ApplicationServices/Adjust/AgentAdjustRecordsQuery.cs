#nullable enable

using System;
using System.Collections.Generic;
using Tgs.CashQueryService;

namespace TGS.Services.BackOffice.ApplicationServices.Adjust;

public record AgentAdjustRecordsQuery(
    Guid BrandId,
    (DateTime From, DateTime To) CreatedOnRange,
    IEnumerable<AdjustType>? SearchTypes,
    Guid? AgentId,
    string? Operator,
    int Skip = 0);
