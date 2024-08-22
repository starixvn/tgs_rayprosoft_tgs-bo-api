using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;

namespace TGS.Apis.BackendOffice.Workers;

public class StationLetterTaskQueue : IStationLetterTaskQueue
{
    private readonly ILogger<StationLetterTaskQueue> _logger;
    private readonly Channel<StationLetterQueueModel> _queue;
    private readonly IServiceProvider _serviceProvider;

    public StationLetterTaskQueue(int capacity, IServiceProvider serviceProvider, ILogger<StationLetterTaskQueue> logger)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _queue = Channel.CreateBounded<StationLetterQueueModel>(options);

        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<StationLetterQueueModel> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }

    public async Task ProcessLoopAsync(CancellationToken cancellationToken = default)
    {
        while (await _queue.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            if (_queue.Reader.TryRead(out var item))
            {
                try
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();

                    var repository = scope.ServiceProvider.GetRequiredService<ITgsRepository>();

                    const string spName = "dbo.station_letter_insert_players";

                    _ = await repository.ExecuteSqlCommandAsync(
                        spName,
                        parameters: new Dictionary<string, object>
                        {
                            { "_brand_id", item.BrandId },
                            { "_station_letter_id", item.StationLetterId }
                        });
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    _logger.LogError(ex, "Error occurred executing task work item.");
                }
            }
        }
    }

    public async ValueTask QueueStationLetterAsync(StationLetterQueueModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        await _queue.Writer.WriteAsync(model);
    }
}
