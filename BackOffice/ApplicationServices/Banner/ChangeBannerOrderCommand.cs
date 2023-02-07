using System;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

public record ChangeBannerOrderCommand(
    Guid BrandId,
    Guid BannerId,
    int OrderNumber,
    string Modifier);
