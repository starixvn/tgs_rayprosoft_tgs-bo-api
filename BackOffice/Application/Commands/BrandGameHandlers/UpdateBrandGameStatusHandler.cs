using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandGame;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameHandlers;

public class UpdateBrandGameStatusHandler(
    IBrandGameService service,
    IRedisCacheManager cacheManager,
    IHttpContextAccessor accessor) 
    : Handler<UpdateBrandGameStatusRequest, MessageResponse>(accessor)
{
    private readonly IRedisCacheManager _cacheManager = cacheManager;
    private readonly IBrandGameService _service = service;

    public override async Task<MessageResponse> Handle(UpdateBrandGameStatusRequest request,
        CancellationToken cancellationToken)
    {
        var brandGame = await _service.GetBrandGameAsync(q => q.Id == request.Id);

        if (brandGame == null)
        {
            return CreateInvalidResponse<MessageResponse>(BrandGame.NotFound);
        }

        brandGame.Status = request.Status;

        await _service.UpdateBrandGameAsync(brandGame);
        await _cacheManager.ClearCacheAsync(RedisCacheType.BrandGames, brandGame.BrandId);

        return CreateValidResponse<MessageResponse>();
    }
}
