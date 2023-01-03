namespace TGS.Services.BackOffice.Application.Models.PlayerIp;

public record PlayerIpModel
{
    public string IpAddress { get; init; }
    public int MemberCount { get; init; }
}
