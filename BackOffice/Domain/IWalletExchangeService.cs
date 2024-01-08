using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;

namespace TGS.Services.BackOffice.Domain;

internal interface IWalletExchangeService
{
    ValueTask CommitWithdrawAsync(
        Guid brandId,
        UserType userType,
        string userId,
        decimal amount,
        decimal actualAmount,
        string reviewUserId,
        Guid formId,
        CancellationToken cancellationToken = default);

    ValueTask RollbackWithdrawAsync(
        Guid brandId,
        UserType userType,
        string userId,
        decimal amount,
        string reviewUserId,
        Guid formId,
        CancellationToken cancellationToken = default);
}
