using System;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

public record BrandGameCreditSuspSettingMergeCommand(
    Guid BrandId,
    decimal LowRisk,
    BrandGameCreditSuspSettingMsg LowNotification,
    decimal ModerateRisk,
    BrandGameCreditSuspSettingMsg ModerateNotification,
    decimal HighRisk,
    BrandGameCreditSuspSettingMsg HighNotification,
    BrandGameCreditSuspSettingMsg ManualNotification);