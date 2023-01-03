namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class UpdatePasswordRequest
{
    public string ConfirmPassword { get; set; }

    public string Password { get; set; }
}
