using System;

namespace TGS.Services.BackOffice.Application.Services;

public record ModifyBrandLogoInfoCommand(
    Guid BrandId,
    Uri DesktopLogoUrl,
    Uri MobileLogoUrl,
    Uri LoadingIconUrl,
    string Modifier);
