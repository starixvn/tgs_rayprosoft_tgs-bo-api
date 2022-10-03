using System;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public record PlayerIpByUserNameModel
{
    public string IpAddress { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}
