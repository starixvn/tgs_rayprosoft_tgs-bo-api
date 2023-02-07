using System;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

public record CreateBannerCommand(
    Guid BrandId,
    string Title,
    DateTimeOffset BeginTime,
    DateTimeOffset? EndTime,
    string Localization,
    string Modifier);
