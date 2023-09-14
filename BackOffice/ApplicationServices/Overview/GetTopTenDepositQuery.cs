using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record GetTopTenDepositQuery(
    Guid BrandId,
    TopTenPlayerSortType SortColumnType,
    bool WinLossAscending);
