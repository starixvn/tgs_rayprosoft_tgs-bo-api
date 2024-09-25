using System;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

public record BrandGameCreditSuspSettingResult(
    Guid ResellerId,
    string ResellerCode,
    Guid LicenseeId,
    string LicenseeCode,
    Guid BrandId,
    string BrandCode,
    decimal LowRisk,
    BrandGameCreditSuspSettingMsg LowNotification,
    decimal ModerateRisk,
    BrandGameCreditSuspSettingMsg ModerateNotification,
    decimal HighRisk,
    BrandGameCreditSuspSettingMsg HighNotification,
    BrandGameCreditSuspSettingMsg ManualNotification,
    DateTimeOffset CreatedOn,
    DateTimeOffset UpdatedOn);
