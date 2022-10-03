using System;

namespace TGS.Services.BackOffice.Application.Services;

public record CreateBannerCommand(
    Guid BrandId,
    string Title,
    DateTimeOffset BeginTime,
    DateTimeOffset? EndTime,
    string Localization,
    string Modifier);
