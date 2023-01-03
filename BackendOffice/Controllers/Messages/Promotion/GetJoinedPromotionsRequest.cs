using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class GetJoinedPromotionsRequest : PageableRequest
{
    public DateTimeOffset From { get; init; }

    public string PlayerCode { get; set; }

    public DateTimeOffset To { get; init; }
}
