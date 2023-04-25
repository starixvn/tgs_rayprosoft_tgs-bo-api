using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Client.Models.AgentWinLoss;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Report;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class AgentWinLossReportController : BaseContextController
{
    [HttpGet("breadcrumb")]
    [Description("Get Agent WinLoss Breadcrumb")]
    public async Task<IEnumerable<AgentWinLossBreadcrumbReportModel>>
        GetBreadcrumbAsync(
            [FromServices]
            IGetService<GetAgentWinLossBreadcrumbReportRequest, IEnumerable<AgentWinLossBreadcrumbReportModel>>
                agentWinLossBreadcrumbService,
            [FromQuery] GetAgentWinLossBreadcrumbReportRequest request)
        => await agentWinLossBreadcrumbService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("agentWinLoss")]
    [Description("Get Agent WinLoss Breadcrumb")]
    public async Task<GetAgentWinLossReportResponse>
        GetBreadcrumbAsync(
            [FromServices] IGetService<GetAgentWinLossReportRequest, GetAgentWinLossReportResponse>
                agentWinLossService,
            [FromQuery] GetAgentWinLossReportRequest request)
        => await agentWinLossService
            .GetAsync(request)
            .ConfigureAwait(false);

}
