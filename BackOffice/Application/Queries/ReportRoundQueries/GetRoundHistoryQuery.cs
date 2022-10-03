#nullable enable
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

public class
    GetRoundHistoryQuery : IGetService<GetRoundHistoryReportRequest,
        JsonDocument?>
{
    private readonly IReportClient _reportClient;

    public GetRoundHistoryQuery(IReportClient reportClient)
        => _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<JsonDocument?> GetAsync(
        GetRoundHistoryReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reportRequest = new GetRoundHistoryRequest
        {
            GameProviderCode = request.GameProviderCode,
            PlayerCode = request.PlayerCode,
            RoundId = request.RoundId,
            Language = request.Language
        };

        return await _reportClient.GetRoundHistoriesAsync(reportRequest);
    }
}