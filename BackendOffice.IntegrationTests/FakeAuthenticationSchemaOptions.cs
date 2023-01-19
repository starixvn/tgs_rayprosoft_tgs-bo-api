using Microsoft.AspNetCore.Authentication;

namespace BackendOffice.IntegrationTests;

internal class FakeAuthenticationSchemaOptions : AuthenticationSchemeOptions
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;
}
