using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record ChangeBannerOrderCommand(
    Guid BrandId,
    Guid BannerId,
    int OrderNumber,
    string Modifier);
