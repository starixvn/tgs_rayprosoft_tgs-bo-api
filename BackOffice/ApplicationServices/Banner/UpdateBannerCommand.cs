using System;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

public record UpdateBannerCommand(
    Guid BrandId,
    Guid BannerId,
    string Title,
    DateTimeOffset BeginTime,
    DateTimeOffset? EndTime,
    string Localization,
    string Modifier);
