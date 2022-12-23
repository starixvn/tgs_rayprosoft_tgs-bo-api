using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandHandlers;

public class UpdateBrandStatusHandler : Handler<UpdateBrandStatusRequest, MessageResponse>
{
    private readonly IBrandService _service;

    public UpdateBrandStatusHandler(IBrandService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandStatusRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _service.GetBrandAsync(q => q.Id == request.BrandId);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        await _service.ChangeStatusAsync(brand, request);

        return CreateValidResponse<MessageResponse>();
    }
}
