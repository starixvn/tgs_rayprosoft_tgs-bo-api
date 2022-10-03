using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;

namespace TGS.Services.BackOffice.Domain;

public interface IBackOfficeRepository
{
    ValueTask ChangeBannerOrderAsync(Guid id, int order, string modifier);

    ValueTask ChangeStatusAsync(Guid id, int status, string modifier);

    ValueTask<Guid> CreateBannerAsync(BannerModifyInfo createInfo, string modifier);

    IAsyncEnumerable<LocalizationStringInfo> ExportLocalizationStringsAsync(string systemCode);

    ValueTask<BannerInfo> GetBannerByIdAsync(Guid id);

    ValueTask<BrandLogoInfo> GetBrandLogoInfoAsync(Guid brandId);

    ValueTask ImportLocalizationStringsAsync(
        string systemCode,
        IEnumerable<LocalizationStringInfo> localizationStrings);

    ValueTask InsertAdjustPlayerRecordAsync(AdjustPlayerRecordCreateInfo createInfo, string modifier, CancellationToken cancellationToken = default);

    ValueTask InsertTransferBalanceFormAsync(
        Guid createId,
        TransferBalanceCreateInfo createInfo,
        Guid modifier,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GameData> QueryAllGamesAsync(Guid brandId);

    IAsyncEnumerable<BannerInfo> QueryExpiredBannersAsync(Guid brandId);

    IAsyncEnumerable<PlayerIdentityAuthenticationInfo> QueryPlayerIdentityAuthenticationsAsync(
        Guid brandId,
        PlayerIdentityReviewState? status,
        string username,
        DateTimeOffset? beginCreatedOn,
        DateTimeOffset? endCreatedOn,
        DateTimeOffset? beginReviewedOn,
        DateTimeOffset? endReviewedOn);

    IAsyncEnumerable<BannerInfo> QueryRejectedBannersAsync(Guid brandId);

    IAsyncEnumerable<BannerInfo> QueryReviewingBannersAsync(Guid brandId);

    IAsyncEnumerable<BannerInfo> QueryValidBannersAsync(Guid brandId);

    ValueTask ReviewBannerAsync(Guid id, BannerReviewState reviewState, string remark, string reviewer);

    ValueTask SetIsEnableBannerAsync(Guid id, bool isEnable, string modifier);

    ValueTask UpdateBannerAsync(Guid id, BannerModifyInfo updateInfo, string modifier);

    ValueTask UpdateBrandLogoInfoAsync(BrandLogoInfo info, string modifier);
}
