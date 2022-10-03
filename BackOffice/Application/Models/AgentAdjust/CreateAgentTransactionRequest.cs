using System;
using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.AgentAdjust;

public class CreateAgentTransactionRequest : IRequest<MessageResponse>
{
    public Guid BrandId { get; set; }
    public Guid AgentId { get; set; }
    public AdjustmentType AdjustmentType { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Remark { get; set; }
}