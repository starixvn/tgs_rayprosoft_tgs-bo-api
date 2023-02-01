using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice;
using TGS.Services.BackOffice.Application.Commands.PlayerIdentity;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

internal class BackOfficeRepository : IBackOfficeRepository
{
    private readonly ISystemClock _systemClock;
    private readonly ITgsRepository _tgsRepository;

    public BackOfficeRepository(
        ITgsRepository tgsRepository,
        ISystemClock systemClock)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    }

    public async ValueTask ChangeBannerOrderAsync(Guid id, int order, string modifier)
    {
        var banner = await _tgsRepository.Banners
            .FindAsync(id)
            .ConfigureAwait(false);

        banner.DisplayOrder = order;

        banner.UpdatedByUserName = modifier;
        banner.UpdatedOn = _systemClock.UtcNow;

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask ChangeStatusAsync(Guid id, int status, string modifier)
    {
        var banner = await _tgsRepository.Banners
            .FindAsync(id)
            .ConfigureAwait(false);

        banner.Status = status;
        banner.UpdatedByUserName = modifier;
        banner.UpdatedOn = DateTimeOffset.UtcNow;

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask<Guid> CreateBannerAsync(BannerModifyInfo createInfo, string modifier)
    {
        var createBanner = new Banner
        {
            Id = Guid.NewGuid(),
            BrandId = createInfo.BrandId,
            BeginTime = createInfo.BeginTime,
            CreatedByUserName = modifier,
            UpdatedByUserName = modifier,
            IsEnabled = true,
            EndTime = createInfo.EndTime,
            Localization = createInfo.Localization,
            Title = createInfo.Title
        };

        _ = await _tgsRepository.Banners
            .AddAsync(createBanner)
            .ConfigureAwait(false);

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);

        return createBanner.Id;
    }

    public IAsyncEnumerable<LocalizationStringInfo> ExportLocalizationStringsAsync(string systemCode)
        => _tgsRepository.SystemLocalizations.AsQueryable()
            .Where(loc => loc.SystemCode == systemCode)
            .Select(loc => new LocalizationStringInfo(
                loc.SystemCode,
                loc.CultureCode,
                loc.MessageId,
                loc.Text))
            .ToAsyncEnumerable();

    public async ValueTask<BannerInfo> GetBannerByIdAsync(Guid id)
    {
        var banner = await _tgsRepository.Banners
            .FindAsync(id)
            .ConfigureAwait(false);

        return new BannerInfo(
            banner.Id,
            banner.BrandId,
            banner.Title,
            banner.BeginTime,
            banner.EndTime,
            banner.Localization,
            ConvertBannerStatus(banner.Status),
            banner.IsEnabled,
            banner.DisplayOrder,
            banner.UpdatedByUserName,
            banner.UpdatedOn);
    }

    public async ValueTask<BrandLogoInfo> GetBrandLogoInfoAsync(Guid brandId)
    {
        var brand = await _tgsRepository.Brands
            .FindAsync(brandId)
            .ConfigureAwait(false);

        return brand == null || string.IsNullOrWhiteSpace(brand.LogoInfo)
            ? null
            : JsonSerializer.Deserialize<BrandLogoInfo>(brand.LogoInfo);
    }

    public async ValueTask ImportLocalizationStringsAsync(
            string systemCode,
        IEnumerable<LocalizationStringInfo> localizationStrings)
    {
        var allLocalizationStrings = _tgsRepository.SystemLocalizations.AsQueryable()
            .Where(loc => loc.SystemCode == systemCode)
            .ToArray();

        var grouping = (from loc in allLocalizationStrings
                        group loc by loc.CultureCode
                        into g
                        select new
                        {
                            CaltureCode = g.Key,
                            Items = g.ToDictionary(loc => loc.MessageId)
                        }).ToDictionary(g => g.CaltureCode, g => g.Items);

        foreach (var info in localizationStrings)
        {
            if (grouping.TryGetValue(info.CultureCode, out var cultureDict)
                && cultureDict.TryGetValue(info.MessageId, out var localization))
            {
                localization.Text = info.Text;
            }
            else
            {
                _ = await _tgsRepository.SystemLocalizations
                    .AddAsync(new SystemLocalization
                    {
                        SystemCode = info.SystemCode,
                        CultureCode = info.CultureCode,
                        MessageId = info.MessageId,
                        Text = info.Text
                    })
                    .ConfigureAwait(false);
            }
        }

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask InsertAdjustPlayerRecordAsync(
        AdjustPlayerRecordCreateInfo createInfo,
        string modifier,
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateTimeOffset.Now;

        var record = new AdjustPlayerRecord
        {
            BrandId = createInfo.BrandId,
            PlayerCode = createInfo.PlayerCode,
            IncludeInNetProfit = createInfo.IncludeInNetProfit,
            AdjustmentType = createInfo.AdjustmentType,
            Type = createInfo.TransactionType,
            TransferType = createInfo.TransferType,
            Amount = createInfo.Amount,
            Remark = createInfo.Remark,
            BucketAmount = createInfo.BucketAmount,
            CreatedByUsername = modifier,
            TransferTarget = createInfo.TargetAgentUserName,
            TransactionId = createInfo.TransactionId,
            TransactionRecordedOn = currentDate,
            CreatedOn = currentDate
        };

        _ = await _tgsRepository.AdjustPlayerRecords
            .AddAsync(record, cancellationToken)
            .ConfigureAwait(false);

        _ = await _tgsRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask InsertTransferBalanceFormAsync(
        Guid createId,
        TransferBalanceCreateInfo createInfo,
        Guid modifier,
        CancellationToken cancellationToken = default)
    {
        var form = new TransferBalanceForm
        {
            Amount = createInfo.Amount,
            BrandId = createInfo.BrandId,
            CreatedUserId = modifier.ToString(),
            Drawee = createInfo.Drawee,
            DraweeType = createInfo.DraweeType,
            Payee = createInfo.Payee,
            PayeeType = createInfo.PayeeType,
            IncludeInNetProfit = createInfo.IncludeInNetProfit,
            Id = createId
        };

        _ = await _tgsRepository.TransferBalanceForms
            .AddAsync(form, cancellationToken)
            .ConfigureAwait(false);

        _ = await _tgsRepository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public IAsyncEnumerable<GameData> QueryAllGamesAsync(Guid brandId)
    {
        var query = from bg in _tgsRepository.BrandGames.AsQueryable()
                    from game in _tgsRepository.Games
                    where bg.BrandId == brandId
                        && bg.GameId == game.Id
                    select new GameData
                    {
                        GameId = game.Id,
                        GameName = game.Name,
                        Localization = game.Localization
                    };

        return query.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<KeyValuePair<string, string>> QueryAllLocalizationStringsAsync(string cultureCode, CancellationToken cancellationToken)
        => _tgsRepository.SystemLocalizations.AsQueryable()
            .Where(loc => loc.SystemCode == "BO-API" && loc.CultureCode == cultureCode)
            .Select(loc => new
            {
                loc.MessageId,
                loc.Text
            })
            .ToArrayAsync(cancellationToken)
            .ToAsyncEnumerable()
            .SelectMany(arr => arr.ToAsyncEnumerable())
            .Select(item => new KeyValuePair<string, string>(item.MessageId, item.Text));

    public IAsyncEnumerable<BannerInfo> QueryExpiredBannersAsync(Guid brandId)
    {
        var currentTime = _systemClock.UtcNow;

        var query = from banner in _tgsRepository.Banners.AsQueryable()
                    where banner.BrandId == brandId
                        && banner.Status == 1000
                        && banner.EndTime <= currentTime
                    select new BannerInfo(
                        banner.Id,
                        banner.BrandId,
                        banner.Title,
                        banner.BeginTime,
                        banner.EndTime,
                        banner.Localization,
                        ConvertBannerStatus(banner.Status),
                        banner.IsEnabled,
                        banner.DisplayOrder,
                        banner.UpdatedByUserName,
                        banner.UpdatedOn);

        return query.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<PlayerIdentityAuthenticationInfo> QueryPlayerIdentityAuthenticationsAsync(
        Guid brandId,
        PlayerIdentityReviewState? status,
        string username,
        DateTimeOffset? beginCreatedOn,
        DateTimeOffset? endCreatedOn,
        DateTimeOffset? beginReviewedOn,
        DateTimeOffset? endReviewedOn)
    {
        var playerQuery = _tgsRepository.Players
            .AsQueryable()
            .Where(p => p.BrandId == brandId);

        if (!string.IsNullOrWhiteSpace(username))
        {
            playerQuery = playerQuery.Where(p => p.Username == username);
        }

        var playerIdentityQuery =
            from playerIdentity in _tgsRepository.PlayerIdentity.AsQueryable()
            join player in playerQuery
                on playerIdentity.PlayerCode equals player.PlayerCode
            join authForms in _tgsRepository.PlayerAuthenticationForms.AsQueryable()
                on playerIdentity.PlayerAuthenticationFormId equals authForms.Id into authFormsGroup
            from authForms in authFormsGroup.DefaultIfEmpty()
            select new
            {
                playerIdentity.PlayerCode,
                playerIdentity.PlayerAuthenticationFormId,
                authForms.Status,
                authForms.CreatedOn,
                authForms.ReviewedOn,
                authForms.Data,
                authForms.Remark,
                player.IdNumber,
                player.RealName,
                player.Username
            };

        if (status != null)
        {
            var queryStatus = (int)status;
            playerIdentityQuery = playerIdentityQuery.Where(p => p.Status == queryStatus);
        }

        if (beginCreatedOn != null && endCreatedOn != null)
        {
            playerIdentityQuery = playerIdentityQuery.Where(paf => paf.CreatedOn > beginCreatedOn && paf.CreatedOn <= endCreatedOn);
        }

        if (beginReviewedOn != null && endReviewedOn != null)
        {
            playerIdentityQuery = playerIdentityQuery.Where(paf => paf.ReviewedOn > beginReviewedOn && paf.ReviewedOn <= endReviewedOn);
        }

        return playerIdentityQuery.Select(info =>
            new PlayerIdentityAuthenticationInfo(
                info.Data,
                info.IdNumber,
                info.RealName,
                info.Remark,
                (PlayerIdentityReviewState)info.Status,
                info.Username,
                info.CreatedOn,
                info.PlayerAuthenticationFormId,
                info.PlayerCode))
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<BannerInfo> QueryRejectedBannersAsync(Guid brandId)
    {
        var query = from banner in _tgsRepository.Banners.AsQueryable()
                    where banner.BrandId == brandId
                        && banner.Status == 2000
                    select new BannerInfo(
                        banner.Id,
                        banner.BrandId,
                        banner.Title,
                        banner.BeginTime,
                        banner.EndTime,
                        banner.Localization,
                        ConvertBannerStatus(banner.Status),
                        banner.IsEnabled,
                        banner.DisplayOrder,
                        banner.UpdatedByUserName,
                        banner.UpdatedOn);

        return query.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<BannerInfo> QueryReviewingBannersAsync(Guid brandId)
    {
        var query = from banner in _tgsRepository.Banners.AsQueryable()
                    where banner.BrandId == brandId
                        && banner.Status == 0
                    select new BannerInfo(
                        banner.Id,
                        banner.BrandId,
                        banner.Title,
                        banner.BeginTime,
                        banner.EndTime,
                        banner.Localization,
                        ConvertBannerStatus(banner.Status),
                        banner.IsEnabled,
                        banner.DisplayOrder,
                        banner.UpdatedByUserName,
                        banner.UpdatedOn);

        return query.ToAsyncEnumerable();
    }

    public IAsyncEnumerable<BannerInfo> QueryValidBannersAsync(Guid brandId)
    {
        var currentTime = _systemClock.UtcNow;

        var query = from banner in _tgsRepository.Banners.AsQueryable()
                    where banner.BrandId == brandId
                        && banner.Status == 1000
                        && (banner.EndTime == null || banner.EndTime > currentTime)
                    select new BannerInfo(
                        banner.Id,
                        banner.BrandId,
                        banner.Title,
                        banner.BeginTime,
                        banner.EndTime,
                        banner.Localization,
                        ConvertBannerStatus(banner.Status),
                        banner.IsEnabled,
                        banner.DisplayOrder,
                        banner.UpdatedByUserName,
                        banner.UpdatedOn);

        return query.ToAsyncEnumerable();
    }

    public async ValueTask ReviewBannerAsync(Guid id, BannerReviewState reviewState, string remark, string reviewer)
    {
        var banner = await _tgsRepository.Banners
            .FindAsync(id)
            .ConfigureAwait(false);

        banner.Status = reviewState switch
        {
            BannerReviewState.Approve => 1000,
            BannerReviewState.Reject => 2000,
            _ => 9000
        };

        banner.UpdatedByUserName = reviewer;
        banner.UpdatedOn = _systemClock.UtcNow;

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask SetIsEnableBannerAsync(Guid id, bool isEnable, string modifier)
    {
        var banner = await _tgsRepository.Banners
            .FindAsync(id)
            .ConfigureAwait(false);

        banner.IsEnabled = isEnable;

        banner.UpdatedByUserName = modifier;
        banner.UpdatedOn = _systemClock.UtcNow;

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask UpdateBannerAsync(Guid id, BannerModifyInfo updateInfo, string modifier)
    {
        var banner = await _tgsRepository.Banners
            .FindAsync(id)
            .ConfigureAwait(false);

        banner.Title = updateInfo.Title;
        banner.BeginTime = updateInfo.BeginTime;
        banner.EndTime = updateInfo.EndTime;
        banner.Localization = updateInfo.Localization;

        banner.UpdatedByUserName = modifier;
        banner.UpdatedOn = _systemClock.UtcNow;

        _ = await _tgsRepository.SaveChangesAsync().ConfigureAwait(false);
    }

    public async ValueTask UpdateBrandLogoInfoAsync(BrandLogoInfo info, string modifier)
    {
        var brand = await _tgsRepository.Brands
            .FindAsync(info.BrandId)
            .ConfigureAwait(false);

        if (brand == null || brand.Id != info.BrandId)
            return;

        brand.LogoInfo = JsonSerializer.Serialize(info);
        brand.UpdatedByUsername = modifier;
        brand.UpdatedOn = DateTimeOffset.UtcNow;

        _ = await _tgsRepository
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async ValueTask UpdateBrandMaintenanceAsync(Guid brandMaintenanceId, string Content)
    {
        var brandMaintenance = await _tgsRepository.BrandMaintenances
            .FirstAsync(bm => bm.Id == brandMaintenanceId)
            .ConfigureAwait(false);

        brandMaintenance.Content = Content;

        _ = await _tgsRepository.SaveChangesAsync();
    }

    private static BannerFlowStatus ConvertBannerStatus(int status)
        => status switch
        {
            0 => BannerFlowStatus.WaitApproving,
            1000 => BannerFlowStatus.Approved,
            2000 => BannerFlowStatus.Rejected,
            _ => BannerFlowStatus.Unknown
        };
}
