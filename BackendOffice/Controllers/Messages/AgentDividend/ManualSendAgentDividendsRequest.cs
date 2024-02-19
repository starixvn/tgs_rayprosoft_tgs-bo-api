
#nullable enable
using System;

namespace TGS.Apis.BackendOffice.Controllers.Messages.AgentDividend;

public record ManualSendAgentDividendsRequest
{
    public Guid[] AgentDividendRecordIds { get; set; } = null!;
    public string? Remark { get; set; }
}