using System;
using Tgs.Logging;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Log;

public class LogRecordRequest : PageableRequest
{
    public DateTimeOffset? From { get; set; }

    public TgsLogLevel Level { get; set; }

    public string ServerName { get; set; }

    public DateTimeOffset? To { get; set; }

    public Guid? TrackingId { get; set; }
}
