using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportExportAgent;

public class
    ExportAgentDividendsTrialCalculationQuery(IReportClient reportClient) : IGetService<ExportAgentDividendsTrialCalculationRequest,
        Stream>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<Stream> GetAsync(ExportAgentDividendsTrialCalculationRequest request,
        CancellationToken cancellationToken = default)
        => await _reportClient.ExportAgentDividendsTrialCalculationAsync(request).ConfigureAwait(false);
}
