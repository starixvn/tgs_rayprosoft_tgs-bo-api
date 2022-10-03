using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Vip;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.ReportExportVipReport;

public class
    ExportVipReportQuery : IGetService<ExportVipReportRequest,
        Stream>
{
    private readonly IReportClient _reportClient;

    public ExportVipReportQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportVipReportRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportVipsAsync(request).ConfigureAwait(false);
}