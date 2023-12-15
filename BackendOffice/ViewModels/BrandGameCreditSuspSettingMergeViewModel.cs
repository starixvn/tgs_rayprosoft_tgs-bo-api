using System;
using System.ComponentModel.DataAnnotations;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Apis.BackendOffice.ViewModels;

public class BrandGameCreditSuspSettingMergeViewModel
{
    [Required]
    [Range(minimum: 0, maximum: 100)]
    public decimal LowRisk { get; init; }
    public BrandGameCreditSuspSettingMsgViewModel LowNotification { get; init; }
    [Required]
    [Range(minimum: 0, maximum: 100)]
    public decimal ModerateRisk { get; init; }
    public BrandGameCreditSuspSettingMsgViewModel ModerateNotification { get; init; }
    [Required]
    [Range(minimum: 0, maximum: 100)]
    public decimal HighRisk { get; init; }
    public BrandGameCreditSuspSettingMsgViewModel HighNotification { get; init; }
    public BrandGameCreditSuspSettingMsgViewModel ManualNotification { get; init; }
}
