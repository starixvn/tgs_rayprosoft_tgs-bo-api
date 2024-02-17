#nullable enable

using System;
using System.Collections.Generic;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Brand;

namespace TGS.Services.BackOffice.ApplicationServices;

public record CreateBrandCommand(
    string Code,
    string Name,
    string? Description,
    string? OperationalEmail,
    decimal PlatformCharge,
    int TimeSpanMinutes,
    bool EnablePlayerIdentity,
    Guid LicenseeId,
    IEnumerable<string> CurrencyCodes,
    IEnumerable<BrandLanguageModel> Languages,
    SettlementIntervalType SettlementIntervalType,
    int? SettlementDay,
    IEnumerable<BrandGameProviderModel> GameProviders,
    AgentDividendType AgentDividendType,
    bool CheckDividendActiveCount,
    bool AutoClearWageringRequirement,
    bool AutoSendAgentBonus,
    bool EnablePlayerAgent,
    decimal AutoClearWageringRequirementBalance,
    decimal DepositHandlingFeeRateTenPercentile,
    decimal WithdrawHandlingFeeRateTenPercentile,
    IEnumerable<string> Domains,
    IEnumerable<BrandBetLimitModel> BetLimits,
    string Modifier);
