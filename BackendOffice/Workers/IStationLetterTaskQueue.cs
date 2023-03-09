using System.Threading;
using System.Threading.Tasks;

namespace TGS.Apis.BackendOffice.Workers;

public interface IStationLetterTaskQueue
{
    ValueTask<StationLetterQueueModel> DequeueAsync(CancellationToken cancellationToken);

    Task ProcessLoopAsync(CancellationToken cancellationToken = default);

    ValueTask QueueStationLetterAsync(StationLetterQueueModel model);
}
