using System;
using TGS.Core.Models.Enums;

namespace TGS.Services.BackOffice.Application.Models.PlayerOnline;

public class PlayerOnlineData
{
    public Guid BrandId { get; init; }

    public string LoginIp { get; init; }

    public DateTimeOffset LoginOn { get; init; }

    public string PlayerCode { get; init; }

    public string RealName { get; init; }

    public PlayerStatus Status { get; init; }

    public string Username { get; init; }

    public Guid VipId { get; init; }

    public int VipLevel { get; init; }
}
