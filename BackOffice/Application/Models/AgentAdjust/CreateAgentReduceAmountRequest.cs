using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.AgentAdjust;

public class CreateAgentReduceAmountRequest : IRequest<MessageResponse>
{
    public AccountType? AccountType { get; set; }

    public AgentReduceTransactionType TransactionType { get; set; }

    public Guid AgentId { get; set; }

    public decimal Amount { get; set; }

    public string Remark { get; set; }

    public string TargetName { get; set; }
}