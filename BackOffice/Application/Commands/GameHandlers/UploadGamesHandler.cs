using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.Game;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.BackOffice.Infrastructure.Services;

namespace TGS.Services.BackOffice.Application.Commands.GameHandlers;

public class UploadGamesHandler(IUploadService uploadService, IGameProviderService gameProviderService,
    IHttpContextAccessor accessor) : Handler<UploadGamesRequest, UploadGamesResponse>(accessor)
{
    public override async Task<UploadGamesResponse> Handle(UploadGamesRequest request,
        CancellationToken cancellationToken)
    {
        var gameProvider = request.Id != null
            ? await gameProviderService.GetGameProviderAsync(x => x.Id == request.Id)
            : null;
        var uploadedResult = await uploadService.GetUploadedExcelAsync<UploadGameModel>(request.File);

        var existedGames = uploadedResult
            .Where(x => gameProvider?.Games.Any(g =>
                g.ExternalId == x.ExternalId &&
                Enum.Parse(typeof(GamePlatformType), x.PlatformType, true).Equals(g.PlatformType)) ?? false)
            .ToArray();

        return existedGames.Length > 0
            ? new()
            {
                Valid = false,
                Message = string.Format(GameProvider.GamesExisted,
                    existedGames.Select(x => x.Code).JoinAsStringByComma())
            }
            : new UploadGamesResponse
            {
                Valid = true,
                Games = [.. uploadedResult]
            };
    }
}
