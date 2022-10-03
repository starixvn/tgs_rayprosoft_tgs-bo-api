using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class GetLicenseesRequest : PageableRequest
{
    public Guid? ResellerId { get; set; }
    public string Keyword { get; set; }
}