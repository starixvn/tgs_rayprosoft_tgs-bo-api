using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;

public class GetBrandContractsRequest : PageableRequest
{
    public Guid BrandId { get; set; }
}
