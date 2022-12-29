using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TGS.Core.Infrastructures.Redis;

namespace TGS.Services.BackOffice.Infrastructure.RedisServices;

public interface IMaintenanceRedisService
{
    ValueTask AddBrandMaintenanceAsync(Guid id);

    ValueTask AddBrandMaintenancesAsync(Guid[] ids);

    ValueTask AddLicenseeMaintenanceAsync(Guid id);

    ValueTask AddLicenseeMaintenancesAsync(Guid[] ids);

    ValueTask AddResellerMaintenanceAsync(Guid id);

    ValueTask AddResellerMaintenancesAsync(Guid[] ids);

    ValueTask ClearResellerMaintenanceAsync(Guid id);

    ValueTask ClearResellerMaintenancesAsync(Guid[] ids);

    ValueTask CleartBrandMaintenanceAsync(Guid id);

    ValueTask CleartBrandMaintenancesAsync(Guid[] ids);

    ValueTask CleartLicenseeMaintenanceAsync(Guid id);

    ValueTask CleartLicenseeMaintenancesAsync(Guid[] ids);

    ValueTask<string> GetBrandMaintenanceAsync(Guid id);

    ValueTask<string> GetLicenseeMaintenanceAsync(Guid id);

    ValueTask<string> GetResellerMaintenanceAsync(Guid ids);
}

public sealed class MaintenanceRedisService : IMaintenanceRedisService
{
    private const string Brands = "Brands";
    private const string Licensees = "Licensees";
    private const string Maintenance = "Maintenance";
    private const string Resellers = "Resellers";
    private readonly IRedisCacheManager _cacheManager;

    public MaintenanceRedisService(IRedisCacheManager cacheManager)
    {
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
    }

    async ValueTask IMaintenanceRedisService.AddBrandMaintenanceAsync(Guid id)
    {
        await AddMaintenanceAsync(Brands, id).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.AddBrandMaintenancesAsync(Guid[] ids)
    {
        await AddMaintenancesAsync(Brands, ids).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.AddLicenseeMaintenanceAsync(Guid id)
    {
        await AddMaintenanceAsync(Licensees, id).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.AddLicenseeMaintenancesAsync(Guid[] ids)
    {
        await AddMaintenancesAsync(Licensees, ids).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.AddResellerMaintenanceAsync(Guid id)
    {
        await AddMaintenanceAsync(Resellers, id).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.AddResellerMaintenancesAsync(Guid[] ids)
    {
        await AddMaintenancesAsync(Resellers, ids).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.ClearResellerMaintenanceAsync(Guid id)
        => await ClearMaintenanceAsync(Resellers, id).ConfigureAwait(false);

    async ValueTask IMaintenanceRedisService.ClearResellerMaintenancesAsync(Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
            return;

        await ClearMaintenancesAsync(Resellers, ids).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.CleartBrandMaintenanceAsync(Guid id)
    {
        await ClearMaintenanceAsync(Brands, id).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.CleartBrandMaintenancesAsync(Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return;
        }

        await ClearMaintenancesAsync(Brands, ids).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.CleartLicenseeMaintenanceAsync(Guid id)
    {
        await ClearMaintenanceAsync(Licensees, id).ConfigureAwait(false);
    }

    async ValueTask IMaintenanceRedisService.CleartLicenseeMaintenancesAsync(Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return;
        }

        await ClearMaintenancesAsync(Licensees, ids).ConfigureAwait(false);
    }

    async ValueTask<string> IMaintenanceRedisService.GetBrandMaintenanceAsync(Guid id)
    {
        return await GetMaintenanceAsync(Brands, id).ConfigureAwait(false);
    }

    async ValueTask<string> IMaintenanceRedisService.GetLicenseeMaintenanceAsync(Guid id)
    {
        return await GetMaintenanceAsync(Licensees, id).ConfigureAwait(false);
    }

    async ValueTask<string> IMaintenanceRedisService.GetResellerMaintenanceAsync(Guid id)
    {
        return await GetMaintenanceAsync(Resellers, id).ConfigureAwait(false);
    }

    private static string GetMaintenanceKey(string merchantKey, Guid id)
    {
        return $"{merchantKey}:{id}";
    }

    private Task AddMaintenanceAsync(string merchantKey, Guid id)
    {
        return _cacheManager.SafeCacheForeverAsync(Maintenance, GetMaintenanceKey(merchantKey, id), id);
    }

    private async Task AddMaintenancesAsync(string merchantKey, Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return;
        }

        var tasks = new List<Task>();
        foreach (var id in ids)
        {
            var task = AddMaintenanceAsync(merchantKey, id);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
    }

    private async ValueTask ClearMaintenanceAsync(string merchantKey, Guid id)
    {
        await _cacheManager.ClearCacheAsync(Maintenance, GetMaintenanceKey(merchantKey, id)).ConfigureAwait(false);
    }

    private async Task ClearMaintenancesAsync(string merchantKey, Guid[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return;
        }

        var cacheKeys = (from id in ids.ToList()
                         select new CacheKey()
                         {
                             CacheType = Maintenance,
                             Key = GetMaintenanceKey(merchantKey, id)
                         }).ToArray();
        _ = await _cacheManager.ClearCacheByBatchAsync(cacheKeys).ConfigureAwait(false);
    }

    private async ValueTask<string> GetMaintenanceAsync(string merchantKey, Guid id)
    {
        return await _cacheManager.SafeGetAsync(Maintenance, GetMaintenanceKey(merchantKey, id), () => Task.FromResult(default(string))).ConfigureAwait(false);
    }
}
