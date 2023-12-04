using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using AllAgentGameType = TGS.Core.Models.Enums.AllAgentGameType;
using GetBrandAllAgentRequest = TGS.Services.BackOffice.Application.Models.BrandAllAgent.GetBrandAllAgentRequest;
using SettlementIntervalType = TGS.Core.Models.Enums.SettlementIntervalType;

namespace TGS.Services.BackOffice.Application.Queries.BrandAllAgentHandlers;

public class GetBrandAllAgentQueryHandler(IBrandAllAgentClient client, IHttpContextAccessor accessor) : Handler<GetBrandAllAgentRequest, GetBrandAllAgentResponse>(accessor)
{
    private readonly IBrandAllAgentClient _client = client;

    public override async Task<GetBrandAllAgentResponse> Handle(GetBrandAllAgentRequest request,
        CancellationToken cancellationToken)
    {
        var brandAllAgent =
            await _client.GetBrandAllAgentAsync(
                new Tgs.Player.All.Agent.Client.Models.GetBrandAllAgentRequest(request.BrandId));

        var brandAllAgentGameTypes = await _client
            .GetGameTypesAsync(new GetGameTypesRequest(request.BrandId))
            .ToArrayAsync(cancellationToken);

        return new GetBrandAllAgentResponse
        {
            Switch = brandAllAgent.Switch,
            SettlementIntervalType = (SettlementIntervalType?)brandAllAgent.SettlementIntervalType,
            BrandAllAgentGameTypes = brandAllAgentGameTypes.Select(x => new BrandAllAgentGameTypeModel
                {
                    Switch = x.Switch,
                    HasValidBet = x.HasValidBet,
                    AllAgentGameType = (AllAgentGameType)x.AllAgentGameType
                })
                .ToArray()
        };
    }
}