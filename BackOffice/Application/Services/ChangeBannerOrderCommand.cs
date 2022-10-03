using System;

namespace TGS.Services.BackOffice.Application.Services;

public record ChangeBannerOrderCommand(
    Guid BrandId,
    Guid BannerId,
    int OrderNumber,
    string Modifier);
