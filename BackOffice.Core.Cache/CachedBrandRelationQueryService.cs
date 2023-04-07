using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Cache;

internal class CachedBrandRelationQueryService : 
    IQueryService<BrandRelationQuery, BrandRelationInfo>,
    IClearBrandRelationCacheService
{
    private readonly IQueryService<BrandRelationQuery, BrandRelationInfo> _sourceService;
    private readonly IMemoryCache _memoryCache;

    public CachedBrandRelationQueryService(
        IQueryService<BrandRelationQuery, BrandRelationInfo> sourceService,
        IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async IAsyncEnumerable<BrandRelationInfo> QueryAsync(
        BrandRelationQuery query, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var key = "Tgs:Brand:Relations";

        var result = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1D);

                return await _sourceService
                    .QueryAsync(query, cancellationToken)
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result!)
            yield return item;
    }

    public ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        var key = "Tgs:Brand:Relations";

        _memoryCache.Remove(key);

        return ValueTask.CompletedTask;
    }
}