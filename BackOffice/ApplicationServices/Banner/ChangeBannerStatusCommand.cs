using System;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

public record ChangeBannerStatusCommand(
    Guid BannerId,
    int Status,
    string Modifier);
