using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

public record GetPlayerVipAdjustQuery(
    string PlayerCode,
    Guid BrandId,
    DateTimeOffset From,
    DateTimeOffset To,
    PageableRequest Paging);
