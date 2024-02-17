using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Brand;

public class UpdateBrandRequest : IRequest<MessageResponse>
{
    public bool AutoClearWageringRequirement { get; set; }

    public decimal AutoClearWageringRequirementBalance { get; set; }

    public BrandBetLimitModel[] BetLimits { get; set; }

    public BrandGroupModel[] BrandGroups { get; set; }

    public bool CheckDividendActiveCount { get; set; }

    public string Code { get; set; }

    public string[] CurrencyCodes { get; set; }

    public decimal DepositHandlingFeeRateTenPercentile { get; set; }

    public string Description { get; set; }

    public string[] Domains { get; set; }

    public string EditorUsername { get; set; }

    public bool EnablePlayerIdentity { get; set; }

    public BrandGameProviderModel[] GameProviders { get; set; }

    public Guid Id { get; set; }

    public BrandLanguageModel[] Languages { get; set; }

    public string Name { get; set; }

    public string OperationalEmail { get; set; }

    public decimal PlatformCharge { get; set; }

    public int? SettlementDay { get; set; }

    public SettlementIntervalType SettlementIntervalType { get; set; }

    public decimal WithdrawHandlingFeeRateTenPercentile { get; set; }

    public bool AutoSendAgentBonus { get; set; }

    public bool EnablePlayerAgent { get; set; }
}
