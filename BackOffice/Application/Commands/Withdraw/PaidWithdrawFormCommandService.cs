using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

internal class PaidWithdrawFormCommandService(
    IPaymentServiceClient paymentServiceClient) : ICommandService<PaidWithdrawFormCommand>
{
    public async ValueTask ExecuteAsync(PaidWithdrawFormCommand command, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();

        await paymentServiceClient.ReviewWithdrawFormAsync(
            command.FormId,
            null,
            ReviewState.Paid,
            command.PaidUserId,
            command.PaidUserName,
            null,
            cancellationToken).ConfigureAwait(false);
    }
}
