using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record ModifyBrandLogoInfoCommand(
    Guid BrandId,
    Uri DesktopLogoUrl,
    Uri MobileLogoUrl,
    Uri LoadingIconUrl,
    string Modifier);
