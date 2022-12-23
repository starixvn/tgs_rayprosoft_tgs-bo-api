using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandAllAgentHandlers;

public class GetBrandAllAgentQueryHandler : Handler<GetBrandAllAgentRequest, GetBrandAllAgentResponse>
{
    private readonly IBrandAllAgentGameTypeService _brandAllAgentGameTypeService;
    private readonly IBrandAllAgentService _service;

    public GetBrandAllAgentQueryHandler(IBrandAllAgentService service,
        IBrandAllAgentGameTypeService brandAllAgentGameTypeService, IHttpContextAccessor accessor) : base(accessor)
    {
        _service = service;
        _brandAllAgentGameTypeService = brandAllAgentGameTypeService;
    }

    public override async Task<GetBrandAllAgentResponse> Handle(GetBrandAllAgentRequest request,
        CancellationToken cancellationToken)
    {
        var brandSwitch = await _service.GetBrandAllAgentQueries()
            .AnyAsync(x => x.BrandId == request.BrandId, cancellationToken);

        var gameTypes = _brandAllAgentGameTypeService.GetBrandAllAgentGameTypeQueries()
            .Where(x => x.BrandId == request.BrandId);

        return new GetBrandAllAgentResponse
        {
            Switch = brandSwitch,
            BrandAllAgentGameTypes = Enum.GetValues(typeof(AllAgentGameType))
                .Cast<AllAgentGameType>()
                .GroupJoin(
                    gameTypes,
                    t => t,
                    g => g.AllAgentGameType,
                    (t, g) => new
                    {
                        AllAgentGameType = t,
                        BrandAllAgentGameTypes = g
                    })
                .Select(x => new BrandAllAgentGameTypeModel
                {
                    AllAgentGameType = x.AllAgentGameType,
                    Switch = x.BrandAllAgentGameTypes.FirstOrDefault()?.Switch ?? false,
                    HasValidBet = x.BrandAllAgentGameTypes.FirstOrDefault()?.HasValidBet ?? false,
                    HasWinLoss = x.BrandAllAgentGameTypes.FirstOrDefault()?.HasWinLoss ?? false
                })
                .ToArray()
        };
    }
}
