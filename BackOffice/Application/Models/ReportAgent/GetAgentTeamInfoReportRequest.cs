using System;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Models.ReportAgent;

public class GetAgentTeamInfoReportRequest : PageableRequest
{
    public Guid AgentId { get; set; }
}
