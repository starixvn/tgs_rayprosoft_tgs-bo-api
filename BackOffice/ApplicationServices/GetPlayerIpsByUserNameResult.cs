using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetPlayerIpsByUserNameResult
{
    public string IpAddress { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}
