#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Tgs.CashQueryService;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AdjustRecordQueryViewModel
{
    [FromQuery(Name = "from")]
    public required DateTime From { get; set; }

    [FromQuery(Name = "operator")]
    public string? Operator { get; set; }

    [FromQuery(Name = "type")]
    public IEnumerable<AdjustType>? SearchTypes { get; set; }

    [FromQuery(Name = "to")]
    public required DateTime To { get; set; }

    [FromQuery(Name = "user")]
    public string? UserName { get; set; }
}
