using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Services.BackOffice.Application.Models.BrandGame;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandGameHandlers;

public class GetGameQueryHandler : Handler<GetBrandGameRequest,
    GetBrandGameResponse>
{
    private readonly IBrandGameService _service;

    public GetGameQueryHandler(IBrandGameService service, IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
    }

    public override async Task<GetBrandGameResponse> Handle(
        GetBrandGameRequest request,
        CancellationToken cancellationToken)
    {
        var brandGame = await _service.GetBrandGameAsync(x => x.Id == request.Id, x => x
            .Where(ResellerId != null, r => r.Brand.Licensee.ResellerId == ResellerId)
            .Where(LicenseeId != null, l => l.Brand.LicenseeId == LicenseeId)
            .Where(BrandId != null, b => b.BrandId == BrandId)
        );

        return brandGame == null
            ? CreateInvalidResponse<GetBrandGameResponse>(BrandGame.NotFound)
            : new GetBrandGameResponse
            {
                Valid = true,
                BrandGame = new BrandGameResponseModel
                {
                    Id = brandGame.Id,
                    GameId = brandGame.GameId,
                    GameName = brandGame.Game.Name,
                    Status = brandGame.Status,
                    GameType = brandGame.Game.Type,
                    GameProviderName = brandGame.Game.GameProvider.Name,
                    Platform = brandGame.Game.PlatformType,
                    SupportHtml5 = brandGame.Game.Html5,
                    EffectiveBettingPercentage = brandGame.EffectiveBettingPercentage,
                    Rebate = brandGame.Rebate
                }
            };
    }
}
