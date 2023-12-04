using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Game;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameHandlers;

public class CreateGamesHandler(
    IGameProviderService service,
    IGameService gameService,
    ISubGameProviderService subGameProviderService,
    IHttpContextAccessor accessor) : Handler<CreateGamesRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(CreateGamesRequest request,
        CancellationToken cancellationToken)
    {
        var gameProvider = await service.GetGameProviderAsync(x => x.Id == request.GameProviderId);

        var gameCodesExisted = gameProvider.Games.Select(x => x.Code).Intersect(request.Games.Select(x => x.Code))
            .ToArray();

        if (gameCodesExisted.Length > 0)
        {
            return CreateInvalidResponse<MessageResponse>(string.Format(GameProvider.GameCodesExisted,
                gameCodesExisted.JoinAsStringByComma()));
        }

        var gameExternalIdsExisted = gameProvider.Games.Select(x => x.ExternalId)
            .Intersect(request.Games.Select(x => x.ExternalId)).ToArray();

        if (gameExternalIdsExisted.Length > 0)
        {
            return CreateInvalidResponse<MessageResponse>(string.Format(GameProvider.ExternalIdsExisted,
                gameExternalIdsExisted.JoinAsStringByComma()));
        }

        var games = request.Games.GroupBy(x => x.ExternalId).Where(x => x.Count() > 1)
            .Select(x => x.Key).ToArray();

        if (games.Length > 0)
        {
            return CreateInvalidResponse<MessageResponse>(string.Format(GameProvider.DuplicateGames,
                games.JoinAsStringByComma()));
        }

        var subGameProviders = await subGameProviderService
            .GetSubGameProviderQueries(q => q.Where(x => x.GameProviderId == request.GameProviderId))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        var subGameProviderCodes = subGameProviders.Select(x => x.Code).ToArray();

        var invalidSubGameProviderCode =
            request.Games.Where(x => !subGameProviderCodes.Contains(x.SubGpCode)).Select(x => x.SubGpCode)
                .Distinct()
                .ToArray();

        if (invalidSubGameProviderCode.Length != 0)
            return CreateInvalidResponse<MessageResponse>(string.Format(GameProvider.InvalidSubGameProviderCode,
                invalidSubGameProviderCode.JoinAsStringByComma()));

        await gameService.CreateGamesAsync(gameProvider, request);

        return CreateValidResponse<MessageResponse>();
    }
}
