using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record GetTopTenPlayerQuery(
    Guid BrandId,
    TopTenPlayerSortType SortColumnType,
    bool WinLossAscending);
