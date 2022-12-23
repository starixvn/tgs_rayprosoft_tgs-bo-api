using System;

namespace TGS.Services.BackOffice.Application.Models.PlatformCredit;

public record PlatformCreditModel
{
    public Guid Id { get; init; }
    public string LicenseeName { get; init; }
    public string BrandName { get; init; }
    public string BrandCode { get; init; }
    public decimal Credit { get; init; }
    public decimal Balance { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
    public DateTimeOffset UpdatedOn { get; init; }
    public string CreatedByUsername { get; set; }
    public string UpdatedByUsername { get; set; }
}
