using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;

namespace TGS.Apis.BackendOffice.Workers;

public class StationLetterBackground : BackgroundService
{
    private readonly ILogger<StationLetterBackground> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStationLetterTaskQueue _taskQueue;

    public StationLetterBackground(IStationLetterTaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<StationLetterBackground> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Station letter queued hosted service is running.");

        await BackgroundProcessingAsync(stoppingToken);
    }

    private async Task BackgroundProcessingAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var repository = scope.ServiceProvider.GetRequiredService<ITgsRepository>();

                var request =
                    await _taskQueue.DequeueAsync(stoppingToken);

                const string spName = "dbo.station_letter_insert_players";

                _ = await repository.ExecuteSqlCommandAsync(spName,
                    parameters: new Dictionary<string, object>
                    {
                        { "_brand_id", request.BrandId },
                        { "_station_letter_id", request.StationLetterId }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Player aggregate queued hosted service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}