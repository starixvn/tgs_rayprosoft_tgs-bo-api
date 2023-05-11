using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Market;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportMarket;

public class
    ExportMarketsQuery : IGetService<ExportMarketsRequest,
        Stream>
{
    private readonly IReportClient _reportClient;

    public ExportMarketsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportMarketsRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportMarketsAsync(request).ConfigureAwait(false);
}
