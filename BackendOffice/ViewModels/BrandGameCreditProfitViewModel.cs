using System;

namespace TGS.Apis.BackendOffice.ViewModels;

public record BrandGameCreditProfitViewModel(
    Guid ResellerId,
    string ResellerCode,
    Guid LicenseeId,
    string LicenseeCode,
    Guid BrandId,
    string BrandCode,
    DateOnly Month,
    Guid GameProviderId,
    string GameProviderCode,
    string GameProviderName,
    int Count,
    decimal Bet,
    decimal ValidBet,
    decimal Settle,
    decimal Commission,
    decimal Turnover,
    decimal WinLoss,
    decimal ProfitAmount);
