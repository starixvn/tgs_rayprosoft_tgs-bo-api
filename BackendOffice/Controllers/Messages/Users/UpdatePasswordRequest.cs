namespace TGS.Apis.BackendOffice.Controllers.Messages.Users;

public class UpdatePasswordRequest
{
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
