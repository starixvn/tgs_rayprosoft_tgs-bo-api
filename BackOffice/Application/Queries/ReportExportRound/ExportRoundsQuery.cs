using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportRound;

public class
    ExportRoundsQuery : IGetService<ExportRoundsRequest,
        Stream>
{
    private readonly IReportClient _reportClient;

    public ExportRoundsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportRoundsRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportRoundsAsync(request).ConfigureAwait(false);
}
