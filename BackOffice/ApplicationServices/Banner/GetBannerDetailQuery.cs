using System;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

public record GetBannerDetailQuery(Guid BrandId, Guid BannerId);
