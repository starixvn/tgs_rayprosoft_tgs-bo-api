#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using Tgs.PlayerService.Client;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class PlayerModel
{
    public string? AgentUsername { get; init; }

    public string? AreaCode { get; init; }

    public decimal Balance { get; init; }

    public BankModel[] Banks { get; init; } = [];

    public string? Birthdate { get; init; }

    public string? Email { get; init; }

    public string? FacebookMessenger { get; init; }

    public string? IdNumber { get; set; }

    public string LastLoginIp { get; init; } = default!;

    public DateTimeOffset? LastLoginOn { get; init; }

    public string? Line { get; init; }

    public string? ParentUsername { get; init; }

    public string? Phone { get; init; }

    public string PlayerCode { get; init; } = default!;

    public string? QQ { get; init; }

    public string? RealName { get; init; }

    public DateTimeOffset? RegisteredOn { get; init; }

    public string RegisterHost { get; init; } = default!;

    public string RegisterIp { get; init; } = default!;

    public string? Remark { get; init; }

    public PlayerStatus Status { get; init; }

    public string StatusName { get; init; } = default!;

    public string? Telegram { get; init; }

    public string Username { get; init; } = default!;

    public string? Viber { get; init; }

    public Guid? VipId { get; init; }

    public int? VipLevel { get; init; }

    public decimal WageringRequirement { get; set; }

    public string? WeChat { get; init; }

    public string? WhatsApp { get; init; }

    public required IReadOnlyDictionary<string, string> Properties { get; init; }

    public bool IdentityAuthentication { get; set; }

    public class BankModel
    {
        public string BankType { get; init; } = default!;

        public JsonDocument Detail { get; init; } = default!;

        public Guid Id { get; init; } = default!;

        public bool IsDefault { get; init; }

        public Guid PaymentChannelId { get; init; } = default!;
    }
}
