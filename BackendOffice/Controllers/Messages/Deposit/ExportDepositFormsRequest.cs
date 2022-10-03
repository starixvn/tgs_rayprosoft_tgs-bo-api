using System;
using Microsoft.AspNetCore.Mvc;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public class ExportDepositFormsRequest
{
    [FromQuery(Name = "bc")]
    public DateTimeOffset? BeginCreateOn { get; init; }

    [FromQuery(Name = "bu")]
    public DateTimeOffset? BeginUpdateOn { get; init; }

    [FromQuery(Name = "ec")]
    public DateTimeOffset? EndCreateOn { get; init; }

    [FromQuery(Name = "eu")]
    public DateTimeOffset? EndUpdateOn { get; init; }
}
