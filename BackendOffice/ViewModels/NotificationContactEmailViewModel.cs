using System.ComponentModel.DataAnnotations;

namespace TGS.Apis.BackendOffice.ViewModels;

public record NotificationContactEmailViewModel
{
    [Required]
    public string Name { get; init; }

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; init; }

    public string Remark { get; init; }
}
