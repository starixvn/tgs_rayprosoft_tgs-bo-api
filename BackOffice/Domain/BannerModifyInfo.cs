using System;

namespace TGS.Services.BackOffice.Domain;

public record BannerModifyInfo(
    Guid BrandId,
    string Title,
    DateTimeOffset BeginTime,
    DateTimeOffset? EndTime,
    string Localization);
