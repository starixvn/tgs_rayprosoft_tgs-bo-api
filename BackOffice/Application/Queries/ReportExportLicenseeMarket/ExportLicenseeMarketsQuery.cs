using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.LicenseeMarket;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportLicenseeMarket;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportLicenseeMarket;

public class
    ExportLicenseeMarketsQuery : IGetService<GetLicenseeMarketsReportRequest,
        Stream>
{
    private readonly IReportClient _reportClient;

    public ExportLicenseeMarketsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(GetLicenseeMarketsReportRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportLicenseeMarketsAsync(new ExportLicenseeMarketsRequest
        {
            CurrencyCode = request.CurrencyCode,
            CurrencyCodes = request.CurrencyCodes,
            From = request.From,
            LicenseeId = request.LicenseeId,
            Page = request.Page,
            Rows = request.Rows,
            ShowAll = request.ShowAll,
            SortBy = request.SortBy,
            To = request.To
        }).ConfigureAwait(false);
}
