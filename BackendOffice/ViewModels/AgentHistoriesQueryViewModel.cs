using System;
using Microsoft.AspNetCore.Mvc;

namespace TGS.Apis.BackendOffice.ViewModels;

public class AgentHistoriesQueryViewModel
{
    [FromQuery(Name = "from")]
    public DateTime From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime To { get; set; }
}
