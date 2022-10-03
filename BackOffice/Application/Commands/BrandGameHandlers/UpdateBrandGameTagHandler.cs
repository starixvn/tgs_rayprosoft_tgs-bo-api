using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Extensions;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandGame;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameHandlers;

public class UpdateBrandGameTagHandler : Handler<UpdateBrandGameTagRequest, MessageResponse>
{
    private readonly IRedisCacheManager _cacheManager;
    private readonly IBrandGameService _service;

    public UpdateBrandGameTagHandler(
        IBrandGameService service,
        IHttpContextAccessor accessor,
        IRedisCacheManager cacheManager) : base(accessor)
    {
        _service = service;
        _cacheManager = cacheManager;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandGameTagRequest request,
        CancellationToken cancellationToken)
    {
        var brandGame = await _service.GetBrandGameAsync(q => q.Id == request.Id);

        if (brandGame == null)
        {
            return CreateInvalidResponse<MessageResponse>(BrandGame.NotFound);
        }

        brandGame.Tag = request.Enabled
            ? brandGame.Tag.AddTag(request.GameTag)
            : brandGame.Tag.RemoveTag(request.GameTag);

        await _service.UpdateBrandGameAsync(brandGame);
        await _cacheManager.ClearCacheAsync(RedisCacheType.BrandGames, brandGame.BrandId, true);

        return CreateValidResponse<MessageResponse>();
    }
}