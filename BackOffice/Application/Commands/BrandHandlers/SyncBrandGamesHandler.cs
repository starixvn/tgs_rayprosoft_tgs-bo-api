using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandHandlers;

public class SyncBrandGamesHandler : Handler<SyncGamesRequest, MessageResponse>
{
    private readonly IBrandService _service;

    public SyncBrandGamesHandler(IBrandService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(SyncGamesRequest request,
        CancellationToken cancellationToken)
    {
        await _service.SyncBrandGamesAsync(request.BrandId);

        return CreateValidResponse<MessageResponse>();
    }
}
