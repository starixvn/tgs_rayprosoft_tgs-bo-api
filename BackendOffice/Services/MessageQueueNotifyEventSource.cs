using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Services;

internal class MessageQueueNotifyEventSource : INotifyEventSource, IDisposable
{
    private bool _disposedValue;
    private Subject<BackOfficeNotifyEvent> _eventSubject = new();

    public IObservable<BackOfficeNotifyEvent> EventSource => _eventSubject.AsObservable();

    public void Dispose()
    {
        // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void PublishEvent(BackOfficeNotifyEvent notifyEvent)
        => _eventSubject.OnNext(notifyEvent);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _eventSubject.OnCompleted();
                _eventSubject.Dispose();
            }

            _disposedValue = true;
        }
    }
}
