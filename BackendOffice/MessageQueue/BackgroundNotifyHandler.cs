using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.BrandService.Protos;
using TGS.Apis.BackendOffice.Services;
using TGS.Services.BackOffice.Domain;
using Valhalla.MessageQueue;

namespace TGS.Apis.BackendOffice.MessageQueue;

internal class BackgroundNotifyHandler(
    MessageQueueNotifyEventSource eventSource,
    ILogger<BackgroundNotifyHandler> logger)
    : IMessageHandler<ReadOnlyMemory<byte>>
{
    private readonly MessageQueueNotifyEventSource _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
    private readonly ILogger<BackgroundNotifyHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public ValueTask HandleAsync(
        string subject,
        ReadOnlyMemory<byte> data,
        IEnumerable<MessageHeaderValue> headerValues,
        CancellationToken cancellationToken = default)
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
