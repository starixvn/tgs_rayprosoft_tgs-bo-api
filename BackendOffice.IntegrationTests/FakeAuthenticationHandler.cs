using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackendOffice.IntegrationTests;

internal class FakeAuthenticationHandler(
    IOptionsMonitor<FakeAuthenticationSchemaOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<FakeAuthenticationSchemaOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            new[] {
                new Claim("UserId", Options.UserId.ToString()),
                new Claim("Username", Options.UserName)
            },
            "Fake");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Fake");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
