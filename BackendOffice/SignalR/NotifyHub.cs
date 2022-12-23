using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TGS.Core.Models;
using TGS.Services.BackOffice.Domain;

#nullable enable

namespace TGS.Apis.BackendOffice.SignalR;

public class NotifyHub : Hub
{
    private readonly INotifyEventSource _eventSource;
    private readonly HashSet<IDisposable> _subscriptions = new();

    public NotifyHub(INotifyEventSource eventSource)
    {
        _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
    }

    public override Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(Consts.BackendOffice.UserInfo.UserIdKey)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _ = _subscriptions.Add(_eventSource.EventSource
                .Where(evt => evt.Match("UserId", userId!)
                    && evt.Match("Feature", "BatchAdjustBalance"))
                .Select(evt => evt.Body.Deserialize<Guid>()!)
                .Select(batchId => Observable.FromAsync(async () =>
                {
                    await Clients.Caller.SendAsync(
                        "notify",
                        new
                        {
                            Type = "BatchAdjustBalance",
                            BatchId = batchId
                        },
                        Context.ConnectionAborted).ConfigureAwait(false);
                }))
                .Concat()
                .Subscribe());
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var sub in _subscriptions)
            try
            {
                sub.Dispose();
            }
            catch
            {
            }

        _subscriptions.Clear();

        return base.OnDisconnectedAsync(exception);
    }
}
