using System;
using Tgs.Paging;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

public record GetBannersQuery(
    Guid BrandId,
    BannerStatus BannerStatus,
    Paging<BannerInfo> Paging);
