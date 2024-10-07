namespace TGS.Apis.BackendOffice.ViewModels;

public class UpdateSmsSettingRequest
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Setting { get; set;}
}
