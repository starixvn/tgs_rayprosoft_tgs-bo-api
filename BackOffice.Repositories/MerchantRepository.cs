using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TGS.Core.DataServices.Tgs;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

public class MerchantRepository(
    ITgsRepository tgsRepository,
    IMemoryCache memoryCache) : IMerchantRepository
{
    public async Task<bool> ExistBrandInLicenseesAsync(Guid brandId, Guid licenseeId)
    {
        var key = $"MerchantRepository:ExistBrandInLicensees:{licenseeId}:{brandId}";
        var result = await memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return await tgsRepository.Brands
                .AsNoTracking()
                .AnyAsync(x => x.Id == brandId && x.LicenseeId == licenseeId);
            });
        return result;
    }

    public async Task<bool> ExistBrandInResellersAsync(Guid brandId, Guid resellerId)
    {
        var key = $"MerchantRepository:ExistBrandInResellers:{resellerId}:{brandId}";
        var result = await memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return await tgsRepository.Licensees
                .AsNoTracking()
                .Where(x => x.ResellerId == resellerId)
                .Join(tgsRepository.Brands,
                 licensee => licensee.Id,
                       brand => brand.LicenseeId,
                       (licensee, brand) => new { Brand = brand, Licensee = licensee })
                .AnyAsync(x => x.Brand.Id == brandId && x.Licensee.ResellerId == resellerId);
            });
        return result;
    }

    public async Task<bool> ExistLicenseeInResellersAsync(Guid licenseeId, Guid resellerId)
    {
        var key = $"MerchantRepository:ExistLicenseeInResellers:{resellerId}:{licenseeId}";
        var result = await memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                return await tgsRepository.Licensees
                .AsNoTracking()
                .AnyAsync(x => x.Id == licenseeId && x.ResellerId == resellerId);
            });
        return result;
    }
}