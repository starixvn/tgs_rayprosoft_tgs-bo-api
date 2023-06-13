#nullable enable

using System;
using Microsoft.AspNetCore.Mvc;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AdjustSummaryQueryViewModel
{
    [FromQuery(Name = "from")]
    public required DateTime From { get; set; }

    [FromQuery(Name = "to")]
    public required DateTime To { get; set; }
}
