using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetCompanySpecialDetailQuery(
    IReportClient reportClient) : IGetService<GetCompanyTotalDetailReportRequest,
        IEnumerable<CompanyTotalDetailReportModel>>
{
    public async ValueTask<IEnumerable<CompanyTotalDetailReportModel>> GetAsync(
        GetCompanyTotalDetailReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await reportClient.GetCompanyTotalDetailAsync(new()
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
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