using System;

namespace TGS.Services.BackOffice.Application.Services;

public record ChangeBannerStatusCommand(
    Guid BannerId,
    int Status,
    string Modifier);