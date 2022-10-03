using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class GetPromotionsRequest : PageableRequest
{
    public Guid BrandId { get; set; }
    public string Type { get; set; }
    public SwitchStatus? Status { get; set; }
    public string Name { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}