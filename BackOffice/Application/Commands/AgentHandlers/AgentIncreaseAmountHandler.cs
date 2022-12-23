using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using TGS.Core.Infrastructures.Json;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.AgentAdjust;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.WalletService;

namespace TGS.Services.BackOffice.Application.Commands.AgentHandlers;

public class AgentIncreaseAmountHandler : Handler<CreateAgentIncreaseAmountRequest, MessageResponse>
{
    private readonly IAdjustAgentAccountService _adjustAgentAccountService;
    private readonly IAgentService _agentService;
    private readonly IAgentWalletRepository _agentWalletRepository;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;

    public AgentIncreaseAmountHandler(
        IAgentService agentService,
        IAgentWalletRepository agentWalletRepository,
        IJsonSerializationProvider jsonSerializationProvider,
        IAdjustAgentAccountService adjustAgentAccountService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _agentService = agentService;
        _agentWalletRepository = agentWalletRepository;
        _jsonSerializationProvider = jsonSerializationProvider;
        _adjustAgentAccountService = adjustAgentAccountService;
    }

    public override async Task<MessageResponse> Handle(
        CreateAgentIncreaseAmountRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.TransactionType) ||
            !Enum.IsDefined((AgentAdjustmentTransactionType)request.TransactionType))
        {
            return CreateInvalidResponse<MessageResponse>("is not define type");
        }

        var agent = await _agentService.GetAgentAsync(x => x.Id == request.AgentId);

        if (agent is null)
        {
            return CreateInvalidResponse<MessageResponse>(Agent.NotFound);
        }

        var remark = _jsonSerializationProvider.SerializeToString(new
        {
            request.TransactionType,
            request.Remark
        });

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var (transactionId, postBalance) = await _agentWalletRepository.ExchangeAsync(
            agent.BrandId,
            agent.DefaultWalletId,
            agent.Id,
            TransactionTypes.CompanyAdjust,
            request.Amount,
            remark).ConfigureAwait(false);

        _ = await _adjustAgentAccountService.CreateAsync(
            transactionId,
            agent.BrandId,
            agent.Id,
            AdjustmentType.Add,
            (AgentAdjustmentTransactionType)request.TransactionType,
            AdjustTransferType.None,
            request.Amount,
            request.Remark,
            Username).ConfigureAwait(false);

        scope.Complete();

        return CreateValidResponse<MessageResponse>();
    }
}
