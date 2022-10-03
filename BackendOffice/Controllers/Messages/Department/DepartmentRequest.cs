using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Department;

public class DepartmentRequest
{
    public string Name { get; set; }
    public Guid? ResellerId { get; set; }
    public Guid? LicenseeId { get; set; }
    public Guid? BrandId { get; set; }
}