using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Report;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportAgent;

namespace TGS.Apis.BackendOffice.Controllers.Report;

[Route("api/[controller]")]
[ApiController]
public class AgentReportController : BaseContextController
{
    [HttpGet("exportAgentContributions")]
    [Description("export agent contributions")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportAgentContributionsFileAsync(
        [FromServices] IGetService<ExportAgentContributionsRequest, Stream>
            exportAgentContributionsService,
        [FromQuery] ExportAgentContributionsRequest request)
        => File(
            await exportAgentContributionsService
                .GetAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "AgentContributions.xlsx");

    [HttpGet("exportAgentDividends")]
    [Description("export agent dividends")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportAgentDividendsFileAsync(
        [FromServices] IGetService<ExportAgentDividendsRequest, Stream>
            exportAgentDividendsService,
        [FromQuery] ExportAgentDividendsRequest request)
        => File(
            await exportAgentDividendsService
                .GetAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "AgentDividends.xlsx");

    [HttpGet("exportAgentDividendsTrialCalculation")]
    [Description("export agent dividends trial calculation")]
    [ByPassAdminLog]
    public async Task<IActionResult> ExportAgentDividendsTrialCalculationAsync(
        [FromServices] IGetService<ExportAgentDividendsTrialCalculationRequest, Stream>
            exportAgentDividendsTrialCalculationService,
        [FromQuery] ExportAgentDividendsTrialCalculationRequest request)
        => File(
            await exportAgentDividendsTrialCalculationService
                .GetAsync(request)
                .ConfigureAwait(false),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "AgentDividendsTrialCalculation.xlsx");

    [HttpGet("dividendDetails")]
    [Description("Get agent dividend details report")]
    public async Task<PagedResponse<AgentDividendDetailReportModel>>
        GetAgentDividendDetailsAsync(
            [FromServices]
            IGetService<GetAgentDividendDetailsReportRequest, PagedResponse<AgentDividendDetailReportModel>>
                agentDividendDetailsService,
            [FromQuery] GetAgentDividendDetailsReportViewRequest request)
    {
        var reportRequest = new GetAgentDividendDetailsReportRequest
        {
            AgentId= request.AgentId,
            AgentDividendPercentage= request.AgentDividendPercentage,
            From= request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            SortBy = request.SortBy,
            OrderBy = request.OrderBy,
            ShowAll = request.ShowAll
        };

        return await agentDividendDetailsService
            .GetAsync(reportRequest)
            .ConfigureAwait(false);
    }

    [HttpGet("dividends")]
    [Description("Get agent dividends report")]
    public async Task<GetAgentDividendsReportResponse>
        GetAgentDividendsAsync(
            [FromServices] IGetService<GetAgentDividendsReportRequest, GetAgentDividendsReportResponse>
                agentDividendsService,
            [FromQuery] GetAgentDividendsReportRequest request)
        => await agentDividendsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("dividendTeamDetails")]
    [Description("Get agent dividend team details report")]
    public async Task<PagedResponse<AgentDividendTeamDetailReportModel>>
        GetAgentDividendTeamDetailsAsync(
            [FromServices]
            IGetService<GetAgentDividendTeamDetailsReportRequest, PagedResponse<AgentDividendTeamDetailReportModel>>
                agentDividendTeamDetailsService,
            [FromQuery] GetAgentDividendTeamDetailsReportViewRequest request)
    {
        var reportRequest = new GetAgentDividendTeamDetailsReportRequest
        {
            AgentId = request.AgentId,
            AgentDividendPercentage = request.AgentDividendPercentage,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            SortBy = request.SortBy,
            OrderBy = request.OrderBy,
            ShowAll = request.ShowAll
        };

        return await agentDividendTeamDetailsService
            .GetAsync(reportRequest)
            .ConfigureAwait(false);
    }

    [HttpGet("{id}/winLoss")]
    [Description("Get agent player Win/Loss report")]
    public async Task<PagedResponse<AgentPlayerWinLossReportModel>> GetAgentPlayerWinLossAsync(
        Guid id,
        [FromServices] IGetService<GetAgentPlayerWinLossReportRequest, PagedResponse<AgentPlayerWinLossReportModel>>
            agentPlayerWinLossService,
        [FromQuery] GetAgentPlayerWinLossReportViewRequest request)
    {
        var reportRequest = new GetAgentPlayerWinLossReportRequest
        {
            AgentId = request.AgentId,
            AgentDividendPercentage = request.AgentDividendPercentage,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            SortBy = request.SortBy,
            OrderBy = request.OrderBy,
            ShowAll = request.ShowAll
        };

        return await agentPlayerWinLossService
            .GetAsync(reportRequest)
            .ConfigureAwait(false);
    }

    [HttpGet("contributions")]
    [Description("Get agent contributions")]
    public async Task<GetAgentContributionsReportResponse> GetRoundsAsync(
        [FromServices] IGetService<GetAgentContributionsReportRequest, GetAgentContributionsReportResponse>
            agentContributionsService,
        [FromQuery] GetAgentContributionsReportRequest request)
        => await agentContributionsService
            .GetAsync(request)
            .ConfigureAwait(false);

    [HttpGet("AgentDividends")]
    [Description("Get agent dividends report")]
    public async Task<GetMainSubAgentDividendsReportResponse>
        GetSingleAgentDividendsAsync(
            [FromServices] IGetService<GetMainSubAgentDividendsReportRequest, GetMainSubAgentDividendsReportResponse>
                agentDividendsService,
            [FromQuery] GetMainSubAgentDividendsReportViewRequest request)
    {
        var reportRequest = new GetMainSubAgentDividendsReportRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            Username = request.Username,
            Type = request.Type,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            SortBy = request.SortBy,
            OrderBy = request.OrderBy,
            ShowAll = request.ShowAll,
        };

        return await agentDividendsService
            .GetAsync(reportRequest)
            .ConfigureAwait(false);
    }
}
