using System.ComponentModel.DataAnnotations;

namespace TGS.Apis.BackendOffice.ViewModels;

public record BrandGameCreditSuspSettingMsgViewModel
{
    [DataType(DataType.Text)]
    public string EmailSubject { get; init; } = default!;
    [DataType(DataType.Text)]
    public string EmailContent { get; init; } = default!;
}
