using System;

namespace TGS.Services.BackOffice.ApplicationServices;

public record CreateBannerCommand(
    Guid BrandId,
    string Title,
    DateTimeOffset BeginTime,
    DateTimeOffset? EndTime,
    string Localization,
    string Modifier);
