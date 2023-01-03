using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;

internal class GetDailyMarketingGamingDetailQueryService : IQueryService<GetDailyMarketingGamingDetailQuery, PlayerDailyGamingItem>
{
    public IAsyncEnumerable<PlayerDailyGamingItem> QueryAsync(GetDailyMarketingGamingDetailQuery query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
