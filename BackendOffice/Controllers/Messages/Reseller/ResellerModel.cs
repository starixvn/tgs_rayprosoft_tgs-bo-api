using System;
using TGS.Core.Models.Enums;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Reseller;

public class ResellerModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public BusinessType BusinessType { get; set; }
    public Status Status { get; set; }
    public string Description { get; set; }
    public string OperationalEmail { get; set; }
    public ResellerGameProviderModel[] GameProviders { get; set; }
    public ResellerCurrencyModel[] Currencies { get; set; }
    public ResellerLanguageModel[] Languages { get; set; }
    public ResellerBetLimitModel[] BetLimits { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public string CreatedByUsername { get; set; }
    public string UpdatedByUsername { get; set; }
}