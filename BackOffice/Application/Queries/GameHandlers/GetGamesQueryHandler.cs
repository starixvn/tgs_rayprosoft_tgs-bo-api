using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Game;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameHandlers;

public class GetGamesQueryHandler(
    IGameService service,
    IHttpContextAccessor accessor)
    : Handler<GetGamesRequest, PagedResponse<GameModel>>(accessor)
{

    public override Task<PagedResponse<GameModel>> Handle(
        GetGamesRequest request,
        CancellationToken cancellationToken)
        => service.GetGameQueries(q => q
                .Where(g => g.GameProviderId == request.GameProviderId))
            .Where(request.Code, g => g.Code == request.Code)
            .Select(x => new GameModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                SubGpCode = x.SubGameProviderCode,
                Status = x.Status,
                Description = x.Description,
                ExternalId = x.ExternalId,
                EndpointPath = x.EndpointPath,
                RuleUrl = x.RuleUrl,
                Type = x.Type,
                PlatformType = x.PlatformType,
                SubGame = x.SubGame,
                AvailableToBrand = x.AvailableToBrand,
                HasTurnoverOnTieBet = x.HasTurnoverOnTieBet,
                Html5 = x.Html5,
                AvailableCurrencies = x.AvailableCurrencies,
                CreatedByUsername = x.CreatedByUsername,
                UpdatedByUsername = x.UpdatedByUsername,
                CreatedOn = x.CreatedOn,
                UpdatedOn = x.UpdatedOn
            })
            .CreatePagedResponseAsync(request);
}
