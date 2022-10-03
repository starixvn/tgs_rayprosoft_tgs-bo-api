using System;
using Tgs.Logging;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Log;

public class LogRecordModel
{
    public string Exception { get; set; }

    public Guid Id { get; set; }

    public TgsLogLevel Level { get; set; }

    public string Logger { get; set; }

    public string Message { get; set; }

    public DateTimeOffset RecordedOn { get; set; }

    public string ServerName { get; set; }

    public int TotalCount { get; set; }

    public Guid TrackingId { get; set; }
}
