#nullable enable

using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.Player;

public class GetPlayersRequest : PageableRequest, IRequest<PagedResponse<PlayersModel>>
{
    public string? AgentName { get; init; }

    public string? BankInfoTag { get; init; }

    public string? BankInfoTagType { get; init; }

    public DateTimeOffset? BeginRegisteredOn { get; init; }

    public Guid BrandId { get; init; }

    public DateTimeOffset? EndRegisteredOn { get; init; }

    public string? ParentUsername { get; init; }

    public string? Phone { get; init; }

    public string? PlayerCode { get; init; }

    public string? RealName { get; init; }

    public string? RegisterHost { get; init; }

    public PlayerStatus? Status { get; init; }

    public string? Username { get; init; }

    public Guid? VipId { get; init; }
}
