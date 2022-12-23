using MediatR;
using TGS.Core.Models.Enums;
using TGS.Core.Models.Messages;

namespace TGS.Services.BackOffice.Application.Models.Player;

public record CreatePlayerReduceAmountRequest : IRequest<MessageResponse>
{
    public string PlayerCode { get; set; }

    public bool IncludeInNetProfit { get; set; }

    public PlayerReduceTransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public string Remark { get; set; }

    public decimal BucketAmount { get; set; }
}
