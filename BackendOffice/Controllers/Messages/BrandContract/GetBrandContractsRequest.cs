using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.BrandContract;

public class GetBrandContractsRequest : PageableRequest
{
    public Guid BrandId { get; set; }
}
