using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Promotion;

public class GetValidPromotionsRequest : PageableRequest
{
    public Guid BrandId { get; set; }
    public string PlayerCode { get; set; }
    public string Language { get; set; }
}