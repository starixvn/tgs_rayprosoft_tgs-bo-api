using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Market;
using Tgs.ServiceUtility;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.ReportMarket;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketQueries;

public class
    GetMarketAgentProfitsQuery : IGetService<GetMarketAgentProfitsReportRequest, PagedResponse<MarketAgentProfitReportModel>>
{
    private readonly IReportClient _reportClient;

    public GetMarketAgentProfitsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<PagedResponse<MarketAgentProfitReportModel>> GetAsync(
        GetMarketAgentProfitsReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetMarketAgentProfitsRequest
        {
            BrandId = request.BrandId,
            Date = request.Date,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = (short)request.OrderBy,
            SortBy = request.SortBy,
            ShowAll = true
        };

        var response = await _reportClient.GetMarketAgentProfitsAsync(reportRequest).ConfigureAwait(false);

        return new()
        {
            Valid = response.Valid,
            Message = response.Message,
            TotalCount = response.TotalCount,
            Items = response.Items?.Select(x => new MarketAgentProfitReportModel
            {
                MasterAgentName = x.MasterAgentName,
                Percentage = x.Percentage,
                DirectPlayerNetProfit = x.DirectPlayerNetProfit,
                TeamNetProfit = x.TeamNetProfit,
                CompanyAdjustment = x.CompanyAdjustment,
                AgentProfit = x.AgentProfit,
                CompanyProfit = x.CompanyProfit
            }).ToArray()
        };
    }
}
