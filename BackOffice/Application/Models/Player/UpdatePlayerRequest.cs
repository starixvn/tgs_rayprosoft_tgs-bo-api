#nullable enable

using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class UpdatePlayerRequest : BaseRequest, IRequest<MessageResponse>
{
    public string? AgentUsername { get; init; }

    public string? AreaCode { get; init; }

    public DateOnly? Birthdate { get; init; }

    public string? ConfirmedPassword { get; init; }

    public string? Email { get; init; }

    public string? FacebookMessenger { get; init; }

    public string? Line { get; init; }

    public string? ParentUsername { get; init; }

    public string? Password { get; init; }

    public string? Phone { get; init; }

    public string PlayerCode { get; set; } = default!;

    public string? QQ { get; init; }

    public string? RealName { get; init; }

    public string? Remark { get; init; }

    public PlayerStatus Status { get; init; }

    public string? Telegram { get; init; }

    public string? Type { get; set; } = default!;

    public string? Viber { get; init; }

    public Guid? VipId { get; init; }

    public string? Wechat { get; init; }

    public string? WhatsApp { get; init; }
}
