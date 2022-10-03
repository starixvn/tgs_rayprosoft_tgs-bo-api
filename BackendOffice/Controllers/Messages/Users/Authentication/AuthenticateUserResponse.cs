using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;

public class AuthenticateUserResponse : MessageResponse
{
    public UserInfo UserInfo { get; set; }
}
