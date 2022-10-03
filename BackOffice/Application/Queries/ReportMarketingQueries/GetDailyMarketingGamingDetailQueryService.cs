using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketingQueries;
internal class GetDailyMarketingGamingDetailQueryService : IQueryService<GetDailyMarketingGamingDetailQuery, PlayerDailyGamingItem>
{
    public IAsyncEnumerable<PlayerDailyGamingItem> QueryAsync(GetDailyMarketingGamingDetailQuery query, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
