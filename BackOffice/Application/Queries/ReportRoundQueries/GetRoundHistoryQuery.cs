#nullable enable

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Round;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportRound;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

public class
    GetRoundHistoryQuery(IGameProvidersBackofficePluggableClient client, IReportClient reportClient) : IGetService<GetRoundHistoryReportRequest,
        JsonDocument?>
{
    private readonly IReportClient _reportClient = reportClient ?? throw new ArgumentNullException(nameof(reportClient));

    public async ValueTask<JsonDocument?> GetAsync(
        GetRoundHistoryReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await GetFromSupportedProvidersAsync(request, cancellationToken).ConfigureAwait(false);
        if (result is not null)
        {
            return result;
        }

        var reportRequest = new GetRoundHistoryRequest
        {
            GameProviderCode = request.GameProviderCode,
            PlayerCode = request.PlayerCode,
            RoundId = request.RoundId,
            Language = request.Language
        };

        return await _reportClient.GetRoundHistoriesAsync(reportRequest);
    }

    private async Task<JsonDocument?> GetFromSupportedProvidersAsync(GetRoundHistoryReportRequest request, CancellationToken cancellationToken = default)
    {
        if (request.GameProviderCode.Equals("SABASW", StringComparison.CurrentCultureIgnoreCase))
        {
            return await client.TicketDetailAsync(new()
            {
                GameProviderCode = request.GameProviderCode,
                PlayerCode = request.PlayerCode,
                GameId = request.GameId,
                ExternalRoundId = request.ExternalRoundId,
                SettleDate = request.ExternalCloseOn,
                ResultId = string.Empty,
                BaseCurrency = request.BaseCurrency,
                PlayerUsername = request.PlayerUsername,
                Language = request.Language,
            }, cancellationToken).ConfigureAwait(false);
        }

        if (request.GameProviderCode.Contains("SABAPLUS", StringComparison.CurrentCultureIgnoreCase))
        {
            return await client.TicketDetailAsync(new()
            {
                GameProviderCode = request.GameProviderCode,
                PlayerCode = request.PlayerCode,
                GameId = request.GameId,
                ExternalRoundId = request.ExternalRoundId,
                SettleDate = request.ExternalCloseOn,
                ResultId = string.Empty,
                BaseCurrency = request.BaseCurrency,
                PlayerUsername = request.PlayerUsername,
                Language = request.Language,
            }, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }
}