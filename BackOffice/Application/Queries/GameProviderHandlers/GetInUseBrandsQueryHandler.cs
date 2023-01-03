using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.GameProviderHandlers;

public class
    GetInUseBrandsQueryHandler : Handler<GetInUseBrandsRequest,
        GetInUseBrandsResponse>
{
    private readonly IGameProviderBetLimitService _service;

    public GetInUseBrandsQueryHandler(IGameProviderBetLimitService service, IHttpContextAccessor accessor)

        : base(accessor)
    {
        _service = service;
    }

    public override async Task<GetInUseBrandsResponse> Handle(GetInUseBrandsRequest request,
        CancellationToken cancellationToken) =>
        new()
        {
            Valid = true,
            BrandNames = await _service.GetInUseBrandsAsync(request.Id)
        };
}
