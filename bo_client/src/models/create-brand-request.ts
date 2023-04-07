interface CreateBrandRequest {
    code: string;
    name: string;
    description?: string;
    operationalEmail?: string;
    platformCharge: number;
    timeSpanMinutes: number;
    enablePlayerIdentity: boolean;
    licenseeId: string;
    currencyCodes: string[];
    languages: BrandLanguageModel[];
    settlementIntervalType: string;
    settlementDay?: number;
    gameProviders: BrandGameProviderModel[];
    agentDividendType: string;
    checkDividendActiveCount: boolean;
    autoClearWageringRequirement: boolean;
    autoClearWageringRequirementBalance: number;
    depositHandlingFeeRateTenPercentile: number;
    withdrawHandlingFeeRateTenPercentile: number;
    domains: string[];
    betLimits: BrandBetLimitModel[];
}