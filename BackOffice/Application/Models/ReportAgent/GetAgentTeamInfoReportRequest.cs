using System;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentTeamInfoReportRequest : PageableRequest
{
    public Guid AgentId { get; set; }
}