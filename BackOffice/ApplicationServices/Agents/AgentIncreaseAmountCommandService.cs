using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Agent.Client;
using Tgs.Agent.Client.Models;
using Tgs.CashService.Client;
using Tgs.ServiceUtility;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.ApplicationServices.Agents;

internal class AgentIncreaseAmountCommandService(
    IAgentClient agentClient,
    ICashServiceClient cashServiceClient,
    IAdjustAgentAccountService adjustAgentAccountService) : IGetService<AgentIncreaseAmountCommand, AgentAdjustAmountResult>
{
    private readonly IAgentClient _agentClient = agentClient ?? throw new ArgumentNullException(nameof(agentClient));
    private readonly ICashServiceClient _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
    private readonly IAdjustAgentAccountService _adjustAgentAccountService = adjustAgentAccountService ?? throw new ArgumentNullException(nameof(adjustAgentAccountService));

    public async ValueTask<AgentAdjustAmountResult> GetAsync(
        AgentIncreaseAmountCommand command,
        CancellationToken cancellationToken = default)
    {
        var agent = await _agentClient.GetAgentInfoAsync(
            new GetAgentInfoRequest(command.AgentId),
            cancellationToken).ConfigureAwait(false);

        if (agent is null)
            return new AgentAdjustAmountResult(false, Agent.NotFound);

        var adjustResult = await _cashServiceClient.AdjustCompanyV2Async(
            agent.WalletId.Value,
            command.Amount,
            command.Remark,
            null,
            command.UserId,
            cancellationToken).ConfigureAwait(false);

        _ = await _adjustAgentAccountService.CreateAsync(
            adjustResult.TransactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Add,
            command.TransactionType,
            AdjustTransferType.None,
            command.Amount,
            command.Remark,
            command.UserName).ConfigureAwait(false);

        return new AgentAdjustAmountResult(true, string.Empty);
    }
}
