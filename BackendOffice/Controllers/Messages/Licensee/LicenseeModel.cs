using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Licensee;

public class LicenseeModel
{
    public Guid Id { get; set; }
    public Guid ResellerId { get; set; }
    public BusinessType BusinessType { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public Status Status { get; set; }
    public string Description { get; set; }
    public string OperationalEmail { get; set; }
    public LicenseeGameProviderModel[] GameProviders { get; set; }
    public LicenseeCurrencyModel[] Currencies { get; set; }
    public LicenseeLanguageModel[] Languages { get; set; }
    public LicenseeBetLimitModel[] BetLimits { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public string CreatedByUsername { get; set; }
    public string UpdatedByUsername { get; set; }
}