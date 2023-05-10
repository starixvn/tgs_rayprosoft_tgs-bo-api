#nullable enable

namespace TGS.Apis.BackendOffice.ViewModels;

public class CreateStationLetterViewModel
{
    public required string Content { get; set; }

    public required string[] Receivers { get; set; }

    public required string Subject { get; set; }
}
