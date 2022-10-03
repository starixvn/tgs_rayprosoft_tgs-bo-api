using System;

namespace TGS.Services.BackOffice.Domain;

public record BannerInfo(
    Guid Id,
    Guid BrandId,
    string Title,
    DateTimeOffset BeginTime,
    DateTimeOffset? EndTime,
    string Localization,
    BannerFlowStatus Status,
    bool IsEnabled,
    int DisplayOrder,
    string Updater,
    DateTimeOffset UpdatedOn);
