using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Department;

public class GetDepartmentsRequest
{
    public Guid? ResellerId { get; set; }
    public Guid? LicenseeId { get; set; }
    public Guid? BrandId { get; set; }
    public string Name { get; set; }
}
