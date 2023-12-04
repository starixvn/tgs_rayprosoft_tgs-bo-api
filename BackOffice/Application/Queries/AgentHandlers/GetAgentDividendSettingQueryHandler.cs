using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.Agent;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.AgentHandlers;

public class
    GetAgentDividendSettingQueryHandler(
    IAgentService service,
    IHttpContextAccessor accessor) : Handler<GetAgentDividendSettingRequest, GetAgentDividendSettingResponse>(accessor)
{
    private readonly IAgentService _service = service;

    public override async Task<GetAgentDividendSettingResponse> Handle(GetAgentDividendSettingRequest request,
        CancellationToken cancellationToken)
    {
        var agent = await _service
            .GetAgentAsync(x => x.BrandId == request.BrandId && x.Username == request.Username)
            .ConfigureAwait(false);

        return new GetAgentDividendSettingResponse
        {
            Valid = true,
            DividendPercentage = agent?.DividendPercentage
        };
    }
}
