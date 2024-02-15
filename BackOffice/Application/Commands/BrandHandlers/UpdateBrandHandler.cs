using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandHandlers;

public class UpdateBrandHandler(
    IBrandService service,
    IHttpContextAccessor accessor)
    : Handler<UpdateBrandRequest, MessageResponse>(accessor)
{
    private readonly IBrandService _service = service;

    public override async Task<MessageResponse> Handle(UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _service.GetBrandAsync(q => q.Id == request.Id);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        if (await _service.ExistsAsync(x => request.Id != x.Id && x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NameExisted);
        }

        if (await _service.ExistsAsync(x => request.Id != x.Id && x.Code == request.Code))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.CodeExisted);
        }

        await _service.UpdateBrandAsync(brand, request);

        return CreateValidResponse<MessageResponse>();
    }
}
