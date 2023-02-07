using System;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

public record SetIsEanbleBannerCommand(
    Guid BrandId,
    Guid BannerId,
    bool IsEnable,
    string Modifier);
