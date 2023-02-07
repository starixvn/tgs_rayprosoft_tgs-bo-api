using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.ApplicationServices.Agents;

namespace TGS.Services.BackOffice.Application.Services;

internal class CachedAgentPlayerDepositQueryService : IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics> _sourceService;

    public CachedAgentPlayerDepositQueryService(
        IQueryService<AgentPlayerDepositQuery, AgentPlayerDepositStatistics> sourceService,
        IMemoryCache memoryCache)
    {
        _sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async IAsyncEnumerable<AgentPlayerDepositStatistics> QueryAsync(
        AgentPlayerDepositQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.Now;
        var recordDate = DateOnly.FromDateTime(now.ToOffset(TimeSpan.FromHours(8 - 7)).DateTime);

        var key = $"AgentPlayerDepositStatistics:{query.BrandId}:{recordDate}";

        var result = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                entry.AbsoluteExpiration = new DateTimeOffset(
                    now.ToUniversalTime().Date.AddDays(1D).AddHours(1),
                    TimeSpan.Zero);

                return await _sourceService
                    .QueryAsync(query, cancellationToken)
                    .ToArrayAsync()
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        foreach (var item in result!)
            yield return item;
    }
}
