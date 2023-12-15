using System;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Apis.BackendOffice.ViewModels;

public record BrandGameCreditSuspSettingViewModel(
    Guid ResellerId,
    string ResellerCode,
    Guid LicenseeId,
    string LicenseeCode,
    Guid BrandId,
    string BrandCode,
    decimal LowRisk,
    BrandGameCreditSuspSettingMsgViewModel LowNotification,
    decimal ModerateRisk,
    BrandGameCreditSuspSettingMsgViewModel ModerateNotification,
    decimal HighRisk,
    BrandGameCreditSuspSettingMsgViewModel HighNotification,
    BrandGameCreditSuspSettingMsgViewModel ManualNotification,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn);
