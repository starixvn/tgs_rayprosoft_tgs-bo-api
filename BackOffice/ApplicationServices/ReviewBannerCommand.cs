using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

public record ReviewBannerCommand(
    Guid BrandId,
    Guid BannerId,
    BannerReviewState ReviewState,
    string Remark,
    string Reviewer);
