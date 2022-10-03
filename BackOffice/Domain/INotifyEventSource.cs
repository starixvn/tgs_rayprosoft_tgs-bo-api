using System;

namespace TGS.Services.BackOffice.Domain;

public interface INotifyEventSource
{
    IObservable<BackOfficeNotifyEvent> EventSource { get; }
}
