using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BackendOffice.IntegrationTests;

internal class FakeAuthenticationHandler : AuthenticationHandler<FakeAuthenticationSchemaOptions>
{
    public FakeAuthenticationHandler(
        IOptionsMonitor<FakeAuthenticationSchemaOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(new[] { new Claim("UserId", Options.UserId.ToString()) }, "Fake");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Fake");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
