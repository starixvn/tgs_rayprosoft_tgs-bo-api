using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;

internal class CheckOfflineLoginRewardBackgroundProcessCompletedQueryService :
    IGetService<CheckOfflineLoginRewardBackgroundProcessCompletedQuery, bool?>
{
    private readonly IBatchTempRepository _batchTempRepository;

    public CheckOfflineLoginRewardBackgroundProcessCompletedQueryService(
        IBatchTempRepository batchTempRepository)
    {
        _batchTempRepository = batchTempRepository ?? throw new ArgumentNullException(nameof(batchTempRepository));
    }

    public ValueTask<bool?> GetAsync(
        CheckOfflineLoginRewardBackgroundProcessCompletedQuery query,
        CancellationToken cancellationToken = default)
        => _batchTempRepository.GetBatchIsCompletedAsync(query.BatchId, cancellationToken);
}
