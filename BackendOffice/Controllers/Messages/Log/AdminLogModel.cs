using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.Log;

public class AdminLogModel
{
    public Guid TrackingId { get; set; }
    public Guid Id { get; set; }
    public string Ip { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public string Description { get; set; }
    public string Exception { get; set; }
    public string StackTrace { get; set; }
    public string InnerException { get; set; }
    public string ServerName { get; set; }
    public string RequestHeaders { get; set; }
    public string RequestContent { get; set; }
    public string ResponseHeaders { get; set; }
    public string ResponseContent { get; set; }
    public int ResponseCode { get; set; }
    public int ExecutionTimeMs { get; set; }
    public DateTimeOffset RecordedOn { get; set; }
    public int TotalCount { get; set; }
}