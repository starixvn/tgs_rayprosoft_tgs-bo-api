using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;
using Tgs.Player.All.Agent.Client;
using Tgs.Player.All.Agent.Client.Models;
using TGS.Services.BackOffice.Application.Models.BrandAllAgent;
using ValidBetConditionModel = TGS.Services.BackOffice.Application.Models.BrandAllAgent.ValidBetConditionModel;

namespace TGS.Services.BackOffice.Application.Queries.BrandAllAgentHandlers;

public class GetBrandAllAgentValidBetSettingQueryHandler(
    IBrandAllAgentClient client) : IStreamRequestHandler<GetBrandAllAgentValidBetSettingRequest,
    ValidBetConditionModel>
{
    private readonly IBrandAllAgentClient _client = client;

    public IAsyncEnumerable<ValidBetConditionModel> Handle(
        GetBrandAllAgentValidBetSettingRequest request,
        CancellationToken cancellationToken) =>
        _client.GetValidBetConditionsAsync(
                new GetValidBetConditionsRequest(request.BrandId, (AllAgentGameType)request.AllAgentGameType))
            .Select(x => new ValidBetConditionModel
            {
                Rank = x.Rank,
                ActiveNumber = x.ActiveNumber,
                NetProfit = x.NetProfit,
                RebatePercentage = x.RebatePercentage
            });
}