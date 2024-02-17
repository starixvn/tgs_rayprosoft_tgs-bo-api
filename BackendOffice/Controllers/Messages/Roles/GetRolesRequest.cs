using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Roles;

public class GetRolesRequest : PageableRequest
{
    public Guid? BrandId { get; set; }

    public bool? IsRoot { get; set; }

    public Guid? LicenseeId { get; set; }

    public string Name { get; set; }

    public Guid? ResellerId { get; set; }
}
