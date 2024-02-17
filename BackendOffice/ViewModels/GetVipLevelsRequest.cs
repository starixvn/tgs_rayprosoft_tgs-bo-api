using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.ViewModels;

public class GetVipLevelsRequest : PageableRequest
{
    public Guid? BrandId { get; set; }
}
