using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class CreateBrandRequest : IRequest<MessageResponse>
{
    public AgentDividendType AgentDividendType { get; set; }

    public bool AutoClearWageringRequirement { get; set; }

    public decimal AutoClearWageringRequirementBalance { get; set; }

    public BrandBetLimitModel[] BetLimits { get; set; }

    public bool CheckDividendActiveCount { get; set; }

    public string Code { get; set; }

    public string[] CurrencyCodes { get; set; }

    public decimal DepositHandlingFeeRateTenPercentile { get; set; }

    public string Description { get; set; }

    public string[] Domains { get; set; }

    public string EditorUsername { get; set; }

    public bool EnablePlayerIdentity { get; set; }

    public BrandGameProviderModel[] GameProviders { get; set; }

    public BrandLanguageModel[] Languages { get; set; }

    public Guid LicenseeId { get; set; }

    public string Name { get; set; }

    public string OperationalEmail { get; set; }

    public decimal PlatformCharge { get; set; }

    public int? SettlementDay { get; set; }

    public SettlementIntervalType SettlementIntervalType { get; set; }

    public decimal WithdrawHandlingFeeRateTenPercentile { get; set; }

    public int TimeSpanMinutes { get; set; }
}
