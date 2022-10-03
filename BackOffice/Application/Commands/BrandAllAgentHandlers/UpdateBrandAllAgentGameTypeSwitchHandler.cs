using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Infrastructure.DataService;
using Brand = TGS.Services.BackOffice.Application.Resources.Brand;

namespace TGS.Services.BackOffice.Application.Commands.BrandAllAgentHandlers;

public class
    UpdateBrandAllAgentGameTypeGameTypeSwitchHandler : Handler<UpdateBrandAllAgentGameTypeRequest, MessageResponse>
{
    private readonly IBrandService _brandService;
    private readonly IBrandAllAgentGameTypeService _service;

    public UpdateBrandAllAgentGameTypeGameTypeSwitchHandler(IBrandService brandService,
        IBrandAllAgentGameTypeService service,
        IHttpContextAccessor accessor) :
        base(accessor)
    {
        _service = service;
        _brandService = brandService;
    }

    public override async Task<MessageResponse> Handle(UpdateBrandAllAgentGameTypeRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandService.GetBrandAsync(x => x.Id == request.BrandId);

        if (brand == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        var brandAllAgentGameType =
            await _service.GetBrandAllAgentGameTypeAsync(x
                => x.BrandId == request.BrandId && x.AllAgentGameType == request.AllAgentGameType);

        if (brandAllAgentGameType is null)
        {
            await _service.CreateBrandAllAgentGameTypeAsync(
                new BrandAllAgentGameType
                {
                    BrandId = request.BrandId,
                    AllAgentGameType = request.AllAgentGameType,
                    Switch = request.Switch,
                    HasWinLoss = request.HasWinLoss,
                    HasValidBet = request.HasValidBet
                });
        }
        else
        {
            brandAllAgentGameType.Switch = request.Switch;
            brandAllAgentGameType.HasValidBet = request.HasValidBet;
            brandAllAgentGameType.HasWinLoss = request.HasWinLoss;

            await _service.UpdateBrandAllAgentGameTypeSwitchAsync(brandAllAgentGameType);
        }

        return CreateValidResponse<MessageResponse>();
    }
}