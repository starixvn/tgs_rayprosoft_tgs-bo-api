using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class GetJoinedPromotionsRequest : PageableRequest
{
    public string PlayerCode { get; set; }
    public DateTimeOffset From { get; init; }
    public DateTimeOffset To { get; init; }
}