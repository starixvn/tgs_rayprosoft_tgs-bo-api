#nullable enable

using System;
using TGS.Services.BackOffice.Domain.Enums;

namespace TGS.Services.BackOffice.ApplicationServices.Overview;

public record GetTopTenGameQuery(
    Guid BrandId,
    TopTenGameSortType SortType,
    bool Ascending);
