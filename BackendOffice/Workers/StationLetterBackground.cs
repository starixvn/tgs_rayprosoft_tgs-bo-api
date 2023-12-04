using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TGS.Apis.BackendOffice.Workers;

public class StationLetterBackground(IStationLetterTaskQueue taskQueue) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => taskQueue.ProcessLoopAsync(stoppingToken);
}
