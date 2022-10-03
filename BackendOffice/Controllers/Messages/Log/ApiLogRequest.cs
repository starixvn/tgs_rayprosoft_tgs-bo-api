using System;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Log;

public class ApiLogRequest : PageableRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public Guid? TrackingId { get; set; }
    public string Ip { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public string Prefix { get; set; }
    public string ServerName { get; set; }
    public int? ResponseCode { get; set; }
}