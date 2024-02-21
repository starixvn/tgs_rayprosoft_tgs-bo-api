using System;
using TGS.Core.Models.Enums;

#nullable enable

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayersModel
{
    public string? AgentName { get; init; }

    public Guid? IdentityFormId { get; init; }

    public string ParentUsername { get; init; } = default!;

    public string PlayerCode { get; init; } = default!;

    public string? RealName { get; init; } = default!;

    public DateTimeOffset? RegisteredTime { get; init; }

    public string? RegisterHost { get; init; }
    
    public PlayerStatus Status { get; init; }

    public string Username { get; init; } = default!;

    public int? VipLevel { get; init; }

    public Guid? UpgradePlayerAgentId { get; init; }
}
