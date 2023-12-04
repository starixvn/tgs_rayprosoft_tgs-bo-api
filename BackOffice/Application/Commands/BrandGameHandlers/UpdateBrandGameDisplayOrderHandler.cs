using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameHandlers;

public class UpdateBrandGameDisplayOrderHandler(
    IBrandGameService service,
    IHttpContextAccessor accessor,
    IRedisCacheManager cacheManager) : Handler<ChangeOrderRequest, MessageResponse>(accessor)
{
    private readonly IRedisCacheManager _cacheManager = cacheManager;
    private readonly IBrandGameService _service = service;

    public override async Task<MessageResponse> Handle(ChangeOrderRequest request,
        CancellationToken cancellationToken)
    {
        var ids = request.Orders.Select(q => q.Id);
        var games = await _service.GetBrandGameQueries(q => q.Where(x => ids.Contains(x.Id)))
            .ToListAsync(cancellationToken);
        games.ForEach(p => p.DisplayOrder = request.Orders.FirstOrDefault(x => x.Id == p.Id)?.DisplayOrder ?? 99);

        await _service.UpdateBrandGamesAsync(games);

        var cacheKeys = games.Select(g => g.BrandId).Distinct().Select(brandId => new CacheKey
        {
            CacheType = RedisCacheType.BrandGames,
            Key = brandId
        }).ToArray();

        _ = await _cacheManager.ClearCacheByBatchAsync(cacheKeys, true);

        return CreateValidResponse<MessageResponse>();
    }
}
