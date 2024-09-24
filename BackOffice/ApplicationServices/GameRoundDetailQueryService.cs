using System.Threading;
using System.Threading.Tasks;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.ReportRound;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.DataService.Services
{
    public class GameRoundDetailQueryService(IGameProvidersBackofficePluggableClient gameProvidersClient) : IGetService<GetRoundHistoryReportRequest, GameRoundDetailResult>
    {
        public async ValueTask<GameRoundDetailResult> GetAsync(GetRoundHistoryReportRequest query, CancellationToken cancellationToken = default)
        {
            return query.GameProviderCode switch
            {
                "SABASW" => CreateSingleResult("Iframe", await gameProvidersClient.TicketDetailUrlAsync(new()
                {
                    GameProviderCode = query.GameProviderCode,
                    PlayerCode = query.PlayerCode,
                    ExternalId = long.Parse(query.ExternalId),
                    Language = query.Language,
                    BaseCurrency = query.BaseCurrency,
                    PlayerUsername = query.PlayerUsername,
                }, cancellationToken).ConfigureAwait(false)),
                _ => null,
            };
        }

        private GameRoundDetailResult CreateSingleResult(string type, object value)
        {
            return new GameRoundDetailResult()
            {
                Data = [
                    new ()
                    {
                        Type = type,
                        Value = value,
                    }
                ]
            };
        }
    }
}