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

public class OfflinePromotionNotifyConsumer(
    INotifyEventSource eventSource,
    IHubContext<NotifyHub> hubContext,
    ILogger<OfflinePromotionNotifyConsumer> logger) : BackgroundService
{
    private readonly INotifyEventSource _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
    private readonly IHubContext<NotifyHub> _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    private readonly ILogger<OfflinePromotionNotifyConsumer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => _eventSource.EventSource
            .Where(evt => evt.Match("Feature", "OfflinePromotion"))
            .Do(evt =>
            {
                _logger.LogDebug("Notify event： {evt}", evt);
            })
            .Select(evt => (
                Payload: evt.Body.Deserialize<OfflineParsePlayerCodeNotifyBody>(BackOfficeUtility.SerializerOptions)!,
                Users: evt.GetTagValues("UserId")))
            .Select(tup => Observable.FromAsync(async () =>
            {
                await _hubContext.Clients.Users(tup.Users).SendAsync(
                    "notify",
                    new
                    {
                        Type = "OfflinePromotion",
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
