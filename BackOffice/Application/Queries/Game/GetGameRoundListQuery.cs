using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Queries.Game;

public record GetGameRoundListQuery(
    Guid BrandId,
    string Username,
    string PlayerCode,
    Guid? RoundId,
    string ExternalRoundId,
    Guid? GameProviderId,
    ClientRoundStatus? Status,
    GameType? GameType,
    DateTimeOffset? From,
    DateTimeOffset? To,
    DateTimeOffset? ClosedFrom,
    DateTimeOffset? ClosedTo,
    PageableRequest Paging);
