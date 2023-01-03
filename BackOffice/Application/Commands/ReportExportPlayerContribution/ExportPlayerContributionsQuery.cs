using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.ReportExportPlayerContribution;

public class
    ExportPlayerContributionsQuery : IGetService<ExportPlayerContributionsRequest,
        Stream>
{
    private readonly IReportClient _reportClient;

    public ExportPlayerContributionsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportPlayerContributionsRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportPlayerContributionsAsync(request).ConfigureAwait(false);
}
