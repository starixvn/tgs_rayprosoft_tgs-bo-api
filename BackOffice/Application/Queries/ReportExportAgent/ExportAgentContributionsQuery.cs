using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportAgent;

public class
    ExportAgentContributionsQuery : IGetService<ExportAgentContributionsRequest,
        Stream>
{
    private readonly IReportClient _reportClient;

    public ExportAgentContributionsQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportAgentContributionsRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportAgentContributionsAsync(request).ConfigureAwait(false);
}
