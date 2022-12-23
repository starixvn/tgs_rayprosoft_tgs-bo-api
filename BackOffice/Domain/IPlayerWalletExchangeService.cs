using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;

namespace TGS.Services.BackOffice.Domain;

internal interface IPlayerWalletExchangeService
{
    ValueTask<TransactionResponse> CancelWithdrawalAsync(
        string playerCode,
        decimal amount,
        string description,
        string externalId,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default);

    ValueTask<TransactionResponse> CommitWithdrawalAsync(
        string playerCode,
        decimal amount,
        decimal actualAmount,
        string description,
        string externalId,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default);

    ValueTask<TransactionResponse> RollbackWithdrawalAsync(
        string playerCode,
        decimal amount,
        string description,
        string externalId,
        Guid? createdByUserId,
        CancellationToken cancellationToken = default);
}
