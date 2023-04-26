using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using Tgs.ServiceUtility;

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
    [Description("Get Agent WinLoss")]
    public async Task<GetAgentWinLossReportResponse>
        GetBreadcrumbAsync(
            [FromServices] IGetService<GetAgentWinLossReportRequest, GetAgentWinLossReportResponse>
                agentWinLossService,
            [FromQuery] GetAgentWinLossReportRequest request)
        => await agentWinLossService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("agentPlayerSpecialDetail")]
    [Description("Get Agent Player Special Detail")]
    public async Task<AgentPlayerSpecialDetailReportModel>
        GetAgentPlayerSpecialDetailAsync(
            [FromServices]
            IGetService<GetAgentPlayerSpecialDetailReportRequest, AgentPlayerSpecialDetailReportModel>
                agentPlayerSpecialService,
            [FromQuery] GetAgentPlayerSpecialDetailReportRequest request)
        => await agentPlayerSpecialService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("agentSpecialDetail")]
    [Description("Get Agent Special Detail")]
    public async Task<IEnumerable<AgentSpecialDetailReportModel>>
        GetAgentSpecialDetailAsync(
            [FromServices] IGetService<GetAgentSpecialDetailReportRequest, IEnumerable<AgentSpecialDetailReportModel>>
                agentSpecialService,
            [FromQuery] GetAgentSpecialDetailReportRequest request)
        => await agentSpecialService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("companySpecialDetail")]
    [Description("Get Company Special Detail")]
    public async Task<IEnumerable<CompanySpecialDetailReportModel>>
        GetCompanySpecialDetailAsync(
            [FromServices]
            IGetService<GetCompanySpecialDetailReportRequest, IEnumerable<CompanySpecialDetailReportModel>>
                companySpecialService,
            [FromQuery] GetCompanySpecialDetailReportRequest request)
        => await companySpecialService
            .GetAsync(request)
            .ConfigureAwait(false);
}