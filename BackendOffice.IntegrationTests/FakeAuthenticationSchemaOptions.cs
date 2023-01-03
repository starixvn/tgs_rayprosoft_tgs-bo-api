using Microsoft.AspNetCore.Authentication;

namespace BackendOffice.IntegrationTests;

internal class FakeAuthenticationSchemaOptions : AuthenticationSchemeOptions
{
    public Guid UserId { get; set; }
}
