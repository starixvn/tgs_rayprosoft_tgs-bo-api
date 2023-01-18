using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Services;

internal class CachedPlayerPaymentStatisticsDetailQueryService
    : IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> _sourceService;

    public CachedPlayerPaymentStatisticsDetailQueryService(
        IQueryService<PlayerPaymentStatisticsDetailQuery, PlayerDailyPaymentItem> sourceService,
        IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async IAsyncEnumerable<PlayerDailyPaymentItem> QueryAsync(
        PlayerPaymentStatisticsDetailQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var key = $"PlayerDailyPaymentItem:{query.BrandId}:{query.QueryDate}";

        var result = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1D);

                return await _sourceService
                    .QueryAsync(query, cancellationToken)
                    .ToArrayAsync()
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result!)
            yield return item;
    }
}
