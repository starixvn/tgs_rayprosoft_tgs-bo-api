using System;

namespace TGS.Services.BackOffice.Domain;

public record BrandLogoInfo(
    Guid BrandId,
    Uri DesktopLogoUrl,
    Uri MobileLogoUrl,
    Uri LoadingUrl);
