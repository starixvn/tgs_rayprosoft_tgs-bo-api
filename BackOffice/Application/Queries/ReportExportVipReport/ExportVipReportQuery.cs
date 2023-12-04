using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Vip;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportVipReport;

public class
    ExportVipReportQuery(IReportClient reportClient) : IGetService<ExportVipReportRequest,
        Stream>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportVipReportRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportVipsAsync(request).ConfigureAwait(false);
}
