using System;

namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public record PlayerIpByIpModel
{
    public string PlayerCode { get; init; }
    public string Username { get; init; }
    public DateTimeOffset CreatedOn { get; init; }
}
