using System;

namespace TGS.Services.BackOffice.Application.Services;

public record SetIsEanbleBannerCommand(
    Guid BrandId,
    Guid BannerId,
    bool IsEnable,
    string Modifier);
