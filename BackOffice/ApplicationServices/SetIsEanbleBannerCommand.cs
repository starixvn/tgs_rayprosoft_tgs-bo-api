using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record SetIsEanbleBannerCommand(
    Guid BrandId,
    Guid BannerId,
    bool IsEnable,
    string Modifier);
