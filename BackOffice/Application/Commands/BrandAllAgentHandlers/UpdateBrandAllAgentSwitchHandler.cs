using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Brand = TGS.Services.BackOffice.Application.Resources.Brand;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class UpdateBrandAllAgentSwitchHandler : Handler<UpdateBrandAllAgentSwitchRequest, MessageResponse>
{
    private readonly IBrandService _brandService;
    private readonly IBrandAllAgentService _service;

    public UpdateBrandAllAgentSwitchHandler(IBrandService brandService, IBrandAllAgentService service,
        IHttpContextAccessor accessor)
        : base(accessor)
    {
        _service = service;
        _brandService = brandService;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentSwitchRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.GetBrandAsync(x => x.Id == request.BrandId);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        var brandAllAgent = await _service.GetBrandAllAgentAsync(x => x.BrandId == request.BrandId);

        switch (request.Switch)
        {
            case true when brandAllAgent is null:
                await _service.CreateBrandAllAgentAsync(new BrandAllAgent { BrandId = request.BrandId });
                break;

            case false when brandAllAgent is not null:
                await _service.DeleteBrandAllAgentAsync(brandAllAgent);
                break;
        }

        return CreateValidResponse<MessageResponse>();
    }
}
