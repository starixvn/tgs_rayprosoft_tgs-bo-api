using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Services.BackOffice.Application.Models;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandGameProviderHandlers;
public class UpdateBrandGameProviderOrderHandler(IBrandGameProviderService service, IHttpContextAccessor accessor) : Handler<ChangeOrderBrandGameProviderRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(ChangeOrderBrandGameProviderRequest request,
        CancellationToken cancellationToken)
    {
        var ids = request.Orders.Select(q => q.Id);
        var gameProviders = await service.GetBrandGameProviderQueries(q => q.Where(x => ids.Contains(x.Id)))
            .ToListAsync(cancellationToken);
        gameProviders.ForEach(p => p.DisplayOrder = request.Orders.FirstOrDefault(x => x.Id == p.Id)?.DisplayOrder ?? 99);

        await service.UpdateGameProvidersAsync(gameProviders);

        return CreateValidResponse<MessageResponse>();
    }
}
