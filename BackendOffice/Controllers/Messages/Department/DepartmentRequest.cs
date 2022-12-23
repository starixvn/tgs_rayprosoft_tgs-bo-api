using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Department;

public class DepartmentRequest
{
    public Guid? BrandId { get; set; }

    public Guid? LicenseeId { get; set; }

    public string Name { get; set; }

    public Guid? ResellerId { get; set; }
}
