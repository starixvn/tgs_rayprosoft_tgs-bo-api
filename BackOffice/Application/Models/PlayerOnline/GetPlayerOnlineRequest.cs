#nullable enable

using System;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.PlayerOnline;

public class GetPlayerOnlineRequest : PageableRequest
{
    public Guid BrandId { get; init; }

    public string? PlayerCode { get; init; }

    public PlayerStatus? Status { get; init; }

    public Guid? VipId { get; init; }

    public string UserName { get; init; }
}
