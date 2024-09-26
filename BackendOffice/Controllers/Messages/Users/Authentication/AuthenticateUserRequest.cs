using System.Net.Http;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Users.Authentication;

public class AuthenticateUserRequest : HttpRequestMessage
{
    public string Password { get; set; }

    public string Username { get; set; }

    public string ReferenceId { get; init; }

    public string Token { get; init; }

    public string Timestamp { get; set; }
}
