using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Market;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportMarket;

namespace TGS.Services.BackOffice.Application.Queries.ReportMarketQueries;

public class
    GetAdditionalExpensesQuery(IReportClient reportClient) : IGetService<GetAdditionalExpensesReportRequest,
        GetAdditionalExpensesReportResponse>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<GetAdditionalExpensesReportResponse> GetAsync(
        GetAdditionalExpensesReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetAdditionalExpensesRequest
        {
            BrandId = request.BrandId,
            ReportDate = request.ReportDate
        };

        var response = await _reportClient.GetAdditionalExpensesAsync(reportRequest);

        return new GetAdditionalExpensesReportResponse
        {
            AdditionalExpenses = response.AdditionalExpenses.Select(x => new AdditionalExpensesReportModel
            {
                AdditionalExpensesType = (AdditionalExpensesType)x.AdditionalExpensesType,
                Amount = x.Amount
            }).ToArray()
        };
    }
}
