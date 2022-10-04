using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record ChangeBannerStatusCommand(
    Guid BannerId,
    int Status,
    string Modifier);