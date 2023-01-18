using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class BrandModel
{
    public AgentDividendType AgentDividendType { get; set; }

    public string ApiKey { get; set; }

    public bool AutoClearWageringRequirement { get; set; }

    public decimal AutoClearWageringRequirementBalance { get; set; }

    public BrandBetLimitModel[] BetLimits { get; set; }

    public BrandGroupModel[] BrandGroups { get; set; }

    public BusinessType BusinessType { get; set; }

    public bool CheckDividendActiveCount { get; set; }

    public string ClientId { get; set; }

    public string Code { get; set; }

    public string CreatedByUsername { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public BrandCurrencyModel[] Currencies { get; set; }

    public decimal DepositHandlingFeeRateTenPercentile { get; set; }

    public string Description { get; set; }

    public string[] Domains { get; set; }

    public bool EnablePlayerIdentity { get; set; }

    public BrandGameProviderModel[] GameProviders { get; set; }

    public BrandGameModel[] Games { get; set; }

    public Guid Id { get; set; }

    public BrandLanguageModel[] Languages { get; set; }

    public Guid LicenseeId { get; set; }

    public string LicenseeName { get; set; }

    public string Name { get; set; }

    public string OperationalEmail { get; set; }

    public decimal PlatformCharge { get; set; }

    public string SecretKey { get; set; }

    public int? SettlementDay { get; set; }

    public SettlementIntervalType? SettlementIntervalType { get; set; }

    public BrandStatus Status { get; set; }

    public int TimeSpanMinutes { get; set; }

    public string UpdatedByUsername { get; set; }

    public DateTimeOffset UpdatedOn { get; set; }

    public decimal WithdrawHandlingFeeRateTenPercentile { get; set; }
}
