#nullable enable

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportRound;

namespace TGS.Services.BackOffice.Application.Queries.ReportRoundQueries;

public class
    GetRoundHistoryQuery(IGameProvidersBackofficePluggableClient client) : IGetService<GetRoundHistoryReportRequest, JsonDocument?>
{
    public async ValueTask<JsonDocument?> GetAsync(
        GetRoundHistoryReportRequest request,
        CancellationToken cancellationToken = default)
    {
        return await client.TicketDetailAsync(new()
        {
            ResultId = string.Empty,
            GameProviderCode = request.GameProviderCode,
            PlayerCode = request.PlayerCode,
            GameId = request.GameId,
            ExternalRoundId = request.ExternalRoundId,
            SettleDate = request.ExternalCloseOn,
            BaseCurrency = request.BaseCurrency,
            PlayerUsername = request.PlayerUsername,
            Language = request.Language,
        }, cancellationToken).ConfigureAwait(false);
    }
}