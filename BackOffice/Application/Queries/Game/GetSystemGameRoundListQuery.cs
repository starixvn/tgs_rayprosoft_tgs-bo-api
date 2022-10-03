using System;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

#nullable enable

namespace TGS.Services.BackOffice.Application.Queries.Game;

public record GetSystemGameRoundListQuery(
    string? Username,
    string? PlayerCode,
    string? ExternalRoundId,
    Guid? RoundId,
    DateTimeOffset? ExternalFrom,
    DateTimeOffset? ExternalTo,
    DateTimeOffset? ExternalClosedFrom,
    DateTimeOffset? ExternalClosedTo,
    DateTimeOffset? From,
    DateTimeOffset? To,
    DateTimeOffset? ClosedFrom,
    DateTimeOffset? ClosedTo,
    ClientRoundStatus? Status,
    Guid? BrandId,
    Guid GameProviderId,
    string? SubGameProviderCode,
    GameType? GameType,
    string? CurrencyCode,
    PageableRequest Paging);
