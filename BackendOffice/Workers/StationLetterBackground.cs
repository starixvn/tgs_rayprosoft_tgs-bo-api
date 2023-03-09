using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TGS.Apis.BackendOffice.Workers;

public class StationLetterBackground : BackgroundService
{
    private readonly IStationLetterTaskQueue _taskQueue;

    public StationLetterBackground(IStationLetterTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _taskQueue.ProcessLoopAsync(stoppingToken);
}
