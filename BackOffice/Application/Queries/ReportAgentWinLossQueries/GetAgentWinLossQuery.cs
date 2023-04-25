using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using TGS.Core.Models.Messages;
using Tgs.Report.Client.Models.AgentWinLoss;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Extensions;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentWinLossQuery : IGetService<GetAgentWinLossReportRequest, GetAgentWinLossReportResponse>
{
    private readonly IReportClient _reportClient;
    private readonly IBrandService _brandService;

    public GetAgentWinLossQuery(IReportClient reportClient, IBrandService brandService)
    {
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    }

    public async ValueTask<GetAgentWinLossReportResponse> GetAsync(GetAgentWinLossReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var platformCharge = await _brandService.GetBrandQueries().Where(x => x.Id == request.BrandId)
            .Select(x => x.PlatformCharge).FirstOrDefaultAsync(cancellationToken);

        var reportRequest = new GetAgentWinLossRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            PlatformCharge = platformCharge,
            From = request.From,
            To = request.To,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = request.ShowAll,
        };

        var response = await _reportClient.GetAgentWinLossAsync(reportRequest);

        if (!response.Valid)
        {
            return new GetAgentWinLossReportResponse
            {
                Valid = response.Valid,
                Message = response.Message,
                TotalCount = response.TotalCount,
                Items = Array.Empty<AgentWinLossReportModel>(),
                Total = new AgentWinLossReportTotalModel()
            };
        }

        var pageArray = response.Items?.Select(x
            => new AgentWinLossReportModel
            {
                Id = x.Id,
                Username = x.Username,
                MemberType = x.MemberType,
                Turnover = x.Turnover,
                WinLoss = x.WinLoss,
                AgentWinLoss = x.AgentWinLoss,
                CompanyWinLoss = x.CompanyWinLoss,
                Special = x.Special,
                AgentSpecial = x.AgentSpecial,
                CompanySpecial = x.CompanySpecial,
                AgentTotal = x.AgentTotal,
                CompanyTotal = x.CompanyTotal
            })
            .AsQueryable()
            .CreatePagedResponse(
                new PageableRequest
                {
                    Page = request.Page,
                    Rows = request.Rows,
                    OrderBy = request.OrderBy,
                    SortBy = request.SortBy
                });

        var total = new AgentWinLossReportTotalModel
        {
            Turnover = response.Items.Sum(x => x.Turnover),
            WinLoss = response.Items.Sum(x => x.WinLoss),
            AgentWinLoss = response.Items.Sum(x => x.AgentWinLoss),
            CompanyWinLoss = response.Items.Sum(x => x.CompanyWinLoss),
            Special = response.Items.Sum(x => x.Special),
            AgentSpecial = response.Items.Sum(x => x.AgentSpecial),
            CompanySpecial = response.Items.Sum(x => x.CompanySpecial),
            AgentTotal = response.Items.Sum(x => x.AgentTotal),
            CompanyTotal = response.Items.Sum(x => x.CompanyTotal),
        };

        return new GetAgentWinLossReportResponse
        {
            TotalCount = response.TotalCount,
            Items = pageArray.Items,
            Total = total
        };
    }
}
