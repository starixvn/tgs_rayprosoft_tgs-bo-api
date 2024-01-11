#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.BrandCertificate.Clients;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Infrastructure.DataService;
using AgentLevelType = Tgs.Agent.Client.Models.Enums.AgentLevelType;
using AgentType = Tgs.Agent.Client.Models.Enums.AgentType;

namespace TGS.Services.BackOffice.Application.Commands.PlayerHandlers;

public class UpgradePlayerToAgentHandler(
    IPlayerService service,
    IAgentClient agentClient,
    IHttpContextAccessor accessor)
    : Handler<UpdatePlayerToAgentRequest, MessageResponse>(accessor)
{
    public override async Task<MessageResponse> Handle(
        UpdatePlayerToAgentRequest request,
        CancellationToken cancellationToken)
    {
        var player = await service.GetPlayerAsync(x => x.PlayerCode == request.PlayerCode);

        var agentLevel = player.AgentId == null ? AgentLevelType.Chief : AgentLevelType.Agent;

        _ = await agentClient.CreateAgentByPlayerUpgradeAsync(new CreateAgentByPlayerUpgradeRequest(player.BrandId,
            player.Username,
            player.RealName,
            player.PlayerCode,
            player.DefaultWalletCurrencyCode,
            player.Phone,
            player.Email,
            Username,
            agentLevel,
            AgentType.Cash,
            player.AgentId,
            "player upgrade"),
            cancellationToken);

        return CreateValidResponse<MessageResponse>();
    }
}
