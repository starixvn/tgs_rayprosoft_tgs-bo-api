using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Tgs.CashService.Client;
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
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IJsonSerializationProvider _jsonSerializationProvider;
    private readonly IPlayerService _playerService;

    public AgentIncreaseAmountHandler(
        IAgentService agentService,
        IAgentWalletRepository agentWalletRepository,
        ICashServiceClient cashServiceClient,
        IPlayerService playerService,
        IJsonSerializationProvider jsonSerializationProvider,
        IAdjustAgentAccountService adjustAgentAccountService,
        IHttpContextAccessor accessor) : base(accessor)
    {
        _agentService = agentService;
        _agentWalletRepository = agentWalletRepository;
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerService = playerService;
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

        var response = await _agentWalletRepository.ExchangeAsync(
            agent.BrandId,
            agent.DefaultWalletId,
            agent.Id,
            TransactionTypes.CompanyAdjust,
            request.Amount,
            remark).ConfigureAwait(false);

        await _adjustAgentAccountService.CreateAsync(
            response.TransactionId,
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