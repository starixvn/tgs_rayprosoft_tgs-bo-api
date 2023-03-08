using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using TGS.Apis.BackendOffice.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.MessageQueue;

internal class BackgroundNotifyHandler : IMessageHandler
{
    private readonly MessageQueueNotifyEventSource _eventSource;
    private readonly ILogger<BackgroundNotifyHandler> _logger;

    public BackgroundNotifyHandler(MessageQueueNotifyEventSource eventSource, ILogger<BackgroundNotifyHandler> logger)
    {
        _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var eventData = NotifyEvent.Parser.ParseFrom(data.Span);

        _logger.LogDebug("NotifyEvent: begin receive. {evt}", eventData);

        var officeNotify = new BackOfficeNotifyEvent(
            eventData.Tags.Select(tag => new KeyValuePair<string, string>(tag.Name, tag.Value)),
            eventData.Data);

        _logger.LogDebug("NotifyEvent: received.");

        _eventSource.PublishEvent(officeNotify);

        return ValueTask.CompletedTask;
    }
}
