using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.AgentAdjust;

public class CreateAgentIncreaseAmountRequest : IRequest<MessageResponse>
{
    public AgentIncreaseTransactionType TransactionType { get; set; }

    public Guid AgentId { get; set; }

    public decimal Amount { get; set; }

    public string Remark { get; set; }
}