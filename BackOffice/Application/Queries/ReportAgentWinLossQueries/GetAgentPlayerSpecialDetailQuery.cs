using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.AgentWinLoss;
using TGS.Services.BackOffice.Application.Models.ReportAgentWinLoss;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportAgentWinLossQueries;

public class
    GetAgentPlayerSpecialDetailQuery(
    IReportClient reportClient) : IGetService<GetAgentPlayerSpecialDetailReportRequest,
        AgentPlayerSpecialDetailReportModel>
{
    public async ValueTask<AgentPlayerSpecialDetailReportModel> GetAsync(
        GetAgentPlayerSpecialDetailReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await reportClient.GetAgentPlayerSpecialDetailAsync(new GetAgentPlayerSpecialDetailRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            From = request.From,
            To = request.To
        });

        return new AgentPlayerSpecialDetailReportModel
        {
            Bonus = response.Bonus,
            BonusAdjust = response.BonusAdjust,
            PlatformCharge = response.PlatformCharge,
            ProductFee = response.ProductFee
        };
    }
}