using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class GetLicenseesRequest : PageableRequest
{
    public string Keyword { get; set; }

    public Guid? ResellerId { get; set; }
}
