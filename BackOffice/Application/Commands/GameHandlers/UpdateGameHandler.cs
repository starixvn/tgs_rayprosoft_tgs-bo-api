using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Game;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.GameHandlers;

public class UpdateGameHandler : Handler<UpdateGameRequest, MessageResponse>
{
    private readonly IGameService _gameService;

    public UpdateGameHandler(IGameService gameService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _gameService = gameService;
    }

    public override async Task<MessageResponse> Handle(UpdateGameRequest request,
        CancellationToken cancellationToken)
    {
        var game = await _gameService.GetGameAsync(x => x.Id == request.Id);

        if (game is null)
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.GameRequired);
        }

        if (await _gameService.ExistsAsync(x =>
            x.GameProviderId == game.GameProviderId && x.Code != game.Code && request.ExternalId == x.ExternalId))
        {
            return CreateInvalidResponse<MessageResponse>(GameProvider.ExternalIdExisted);
        }

        game.Name = request.Name;
        game.Status = request.Status;
        game.ExternalId = request.ExternalId;
        game.Description = request.Description;
        game.EndpointPath = request.EndpointPath;
        game.RuleUrl = request.RuleUrl;
        game.Type = request.Type;
        game.PlatformType = request.PlatformType;
        game.SubGame = request.SubGame;
        game.AvailableToBrand = request.AvailableToBrand;
        game.HasTurnoverOnTieBet = request.HasTurnoverOnTieBet;
        game.Html5 = request.Html5;
        game.AvailableCurrencies = request.AvailableCurrencies;
        game.UpdatedByUsername = request.EditorUsername;
        game.UpdatedOn = DateTimeOffset.Now;
        game.SubGameProviderCode = request.SubGpCode;

        await _gameService.UpdateGameAsync(game);

        return CreateValidResponse<MessageResponse>();
    }
}