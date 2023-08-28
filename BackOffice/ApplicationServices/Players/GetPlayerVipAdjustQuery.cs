using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record GetPlayerVipAdjustQuery(
    string PlayerCode,
    Guid BrandId,
    DateTimeOffset From,
    DateTimeOffset To,
    PageableRequest Paging);
