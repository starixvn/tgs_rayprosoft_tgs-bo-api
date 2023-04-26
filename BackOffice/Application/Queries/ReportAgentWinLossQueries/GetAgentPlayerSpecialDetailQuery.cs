using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Client;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentPlayerSpecialDetailQuery : IGetService<GetAgentPlayerSpecialDetailReportRequest,
        AgentPlayerSpecialDetailReportModel>
{
    private readonly IBrandService _brandService;
    private readonly IReportClient _reportClient;

    public GetAgentPlayerSpecialDetailQuery(
        IReportClient reportClient,
        IBrandService brandService)
    {
        _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    }

    public async ValueTask<AgentPlayerSpecialDetailReportModel> GetAsync(
        GetAgentPlayerSpecialDetailReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var platformCharge = await _brandService.GetBrandQueries().Where(x => x.Id == request.BrandId)
            .Select(x => x.PlatformCharge).FirstOrDefaultAsync(cancellationToken);

        var response = await _reportClient.GetAgentPlayerSpecialDetailAsync(new()
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            PlatformCharge = platformCharge,
            From = request.From,
            To = request.To
        });

        return new AgentPlayerSpecialDetailReportModel
        {
            Bonus = response.Bonus,
            BonusAdjust = response.BonusAdjust,
            Special = response.Special
        };
    }
}