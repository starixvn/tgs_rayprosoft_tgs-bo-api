using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.BrandHandlers;

public class CreateBrandHandler : Handler<CreateBrandRequest, MessageResponse>
{
    private readonly IBrandService _service;

    public CreateBrandHandler(IBrandService service, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
    }

    public override async Task<MessageResponse> Handle(CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        if (await _service.ExistsAsync(x => x.Name == request.Name))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NameExisted);
        }

        if (await _service.ExistsAsync(x => x.Code.ToLower() == request.Code.ToLower()))
        {
            return CreateInvalidResponse<MessageResponse>(Brand.CodeExisted);
        }

        await _service.CreateBrandAsync(request);

        return CreateValidResponse<MessageResponse>();
    }
}
