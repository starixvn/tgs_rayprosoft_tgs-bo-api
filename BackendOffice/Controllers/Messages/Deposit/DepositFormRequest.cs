using System;
using Microsoft.AspNetCore.Mvc;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

public record DepositFormRequest
{
    [FromQuery(Name = "username")]
    public string Username { get; init; }

    [FromQuery(Name = "channel")]
    public string ChannelName { get; init; }

    [FromQuery(Name = "thirdOrderNo")]
    public string ThirdPartyOrderNo { get; init; }

    [FromQuery(Name = "id")]
    public Guid? FormId { get; init; }

    [FromQuery(Name = "payment")]
    public Guid? PaymentId { get; init; }

    [FromQuery(Name = "method")]
    public Guid? PaymentMethodId { get; init; }

    [FromQuery(Name = "status")]
    public FlowState[] Status { get; init; }

    [FromQuery(Name = "bc")]
    public DateTimeOffset? BeginCreateOn { get; init; }

    [FromQuery(Name = "ec")]
    public DateTimeOffset? EndCreateOn { get; init; }

    [FromQuery(Name = "bu")]
    public DateTimeOffset? BeginUpdateOn { get; init; }

    [FromQuery(Name = "eu")]
    public DateTimeOffset? EndUpdateOn { get; init; }

    [FromQuery(Name = "orderNo")]
    public string OrderNo { get; init; }
}
