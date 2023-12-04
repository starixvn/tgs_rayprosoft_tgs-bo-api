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
    GetCompanySpecialDetailQuery(
    IReportClient reportClient,
    IBrandService brandService) : IGetService<GetCompanyTotalDetailReportRequest,
        IEnumerable<CompanyTotalDetailReportModel>>
{
    private readonly IBrandService _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<IEnumerable<CompanyTotalDetailReportModel>> GetAsync(
        GetCompanyTotalDetailReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var platformCharge = await _brandService.GetBrandQueries().Where(x => x.Id == request.BrandId)
            .Select(x => x.PlatformCharge).FirstOrDefaultAsync(cancellationToken);

        var response = await _reportClient.GetCompanyTotalDetailAsync(new()
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            PlatformCharge = platformCharge,
            From = request.From,
            To = request.To
        });

        return response.Select(x => new CompanyTotalDetailReportModel
        {
            From = x.From,
            To = x.To,
            Percentage = x.Percentage,
            PlayerWinLoss = x.PlayerWinLoss,
            PlayerSpecial = x.PlayerSpecial,
            CompanyWinLoss = x.CompanyWinLoss,
            CompanySpecial = x.CompanySpecial,
            CompanyTotal = x.CompanyTotal
        });
    }
}