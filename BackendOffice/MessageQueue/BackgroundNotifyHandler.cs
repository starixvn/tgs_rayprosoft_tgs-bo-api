using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandService.Protos;
using Tgs.MessageQueue;
using TGS.Apis.BackendOffice.Services;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.MessageQueue;

internal class BackgroundNotifyHandler : IMessageHandler
{
    private readonly MessageQueueNotifyEventSource _eventSource;

    public BackgroundNotifyHandler(MessageQueueNotifyEventSource eventSource)
    {
        _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var eventData = NotifyEvent.Parser.ParseFrom(data.Span);

        var officeNotify = new BackOfficeNotifyEvent(
            eventData.Tags.Select(tag => new KeyValuePair<string, string>(tag.Name, tag.Value)),
            eventData.Data);

        _eventSource.PublishEvent(officeNotify);

        return ValueTask.CompletedTask;
    }
}
