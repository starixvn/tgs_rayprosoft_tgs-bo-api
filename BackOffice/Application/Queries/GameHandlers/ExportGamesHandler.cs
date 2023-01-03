using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Infrastructures.Utility;
using TGS.Services.BackOffice.Application.Models.Game;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameHandlers;

public class ExportGamesHandler : Handler<ExportGamesRequest, MemoryStream>
{
    private readonly IGameService _gameService;

    public ExportGamesHandler(IGameService gameService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _gameService = gameService;
    }

    public override async Task<MemoryStream> Handle(ExportGamesRequest request,
        CancellationToken cancellationToken)
    {
        var games = await _gameService.GetGameQueries(q => q.Where(x => x.GameProviderId == request.Id))
            .Select(x => new ExportGameModel
            {
                Name = x.Name,
                Code = x.Code,
                RuleUrl = x.RuleUrl,
                ExternalId = x.ExternalId,
                Type = x.Type.ToString(),
                PlatformType = x.PlatformType.ToString(),
                HasTurnoverOnTieBet = x.HasTurnoverOnTieBet,
                EndpointPath = x.EndpointPath,
                Description = x.Description,
                SubGame = x.SubGame,
                AvailableToBrand = x.AvailableToBrand,
                Html5 = x.Html5,
                AvailableCurrencies = x.AvailableCurrencies,
                SubGpCode = x.SubGameProviderCode
            })
            .ToArrayAsync(cancellationToken);

        var xlsx = games.Export("games");
        await using var stream = new MemoryStream();
        xlsx.SaveAs(stream);
        return new MemoryStream(stream.ToArray());
    }
}
