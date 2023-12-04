#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class GetProcessedUsersQueryService(
    IBatchTempRepository batchTempRepository)
        : IGetService<GetProcessedUsersQuery, OfflineUsersResult?>
{
    private readonly IBatchTempRepository _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));

    public ValueTask<OfflineUsersResult?> GetAsync(GetProcessedUsersQuery query, CancellationToken cancellationToken = default)
        => _batchTempRepository.GetOfflineUsersResultAsync(query.BatchId, cancellationToken);
}
