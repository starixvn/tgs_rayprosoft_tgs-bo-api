using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TGS.Apis.BackendOffice.SignalR;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Services;

public class BatchAdjustNotifyConsumer : BackgroundService
{
    private readonly INotifyEventSource _eventSource;
    private readonly IHubContext<NotifyHub> _hubContext;
    private readonly ILogger<BatchAdjustNotifyConsumer> _logger;

    public BatchAdjustNotifyConsumer(
        INotifyEventSource eventSource,
        IHubContext<NotifyHub> hubContext,
        ILogger<BatchAdjustNotifyConsumer> logger)
    {
        _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _eventSource.EventSource
            .Where(evt => evt.Match("Feature", "BatchAdjustBalance"))
            .Do(evt =>
            {
                _logger.LogDebug("Notify event： {evt}", evt);
            })
            .Select(evt => (
                Payload: evt.Body.Deserialize<BatchAdjustBalanceNotifyBody>(BackOfficeUtility.SerializerOptions)!,
                Users: evt.GetTagValues("UserId")))
            .Select(tup => Observable.FromAsync(async () =>
            {
                await _hubContext.Clients.Users(tup.Users).SendAsync(
                    "notify",
                    new
                    {
                        Type = "BatchAdjustBalance",
                        tup.Payload
                    },
                    stoppingToken).ConfigureAwait(false);
            }))
            .Merge()
            .Catch((Exception ex) =>
            {
                _logger.LogError(ex, "Occur error.");

                return Observable.Empty<Unit>();
            })
            .ToTask(stoppingToken);
}
