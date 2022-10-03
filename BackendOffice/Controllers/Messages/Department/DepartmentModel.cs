using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Department;

public class DepartmentModel
{
    public Guid Id { get; init; }
    public Guid? ResellerId { get; init; }
    public Guid? LicenseeId { get; init; }
    public Guid? BrandId { get; init; }
    public string ResellerName { get; init; }
    public string LicenseeName { get; init; }
    public string BrandName { get; init; }
    public string Name { get; init; }
}