using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

internal class ReviewWithdrawFormCommandService(
    IPaymentServiceClient paymentServiceClient,
    IWalletExchangeService walletExchangeService)
    : ICommandService<ReviewWithdrawFormCommand>
{
    public async ValueTask ExecuteAsync(
        ReviewWithdrawFormCommand command,
        CancellationToken cancellationToken = default)
    {
        var formData = await paymentServiceClient
            .GetWithdrawFormAsync(command.BrandId, command.FormId, cancellationToken)
            .ConfigureAwait(false);

        var fsm = paymentServiceClient.CreateWithdrawStateMachine(formData.Status);

        if (command.Status == ReviewState.Approve)
        {
            if (!fsm.CanFire(PaymentTrigger.Approve))
                return;

            await paymentServiceClient.ReviewWithdrawFormAsync(
                command.FormId,
                command.BrandPaymentId.Value,
                command.Status,
                command.ReviewUserId,
                command.ReviewUserName,
                command.Remark,
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if (!fsm.CanFire(PaymentTrigger.Reject))
                return;

            await paymentServiceClient.ReviewWithdrawFormAsync(
                command.FormId,
                command.BrandPaymentId,
                command.Status,
                command.ReviewUserId,
                command.ReviewUserName,
                command.Remark,
                cancellationToken).ConfigureAwait(false);

            await walletExchangeService.RollbackWithdrawAsync(
                formData.BrandId,
                formData.UserType,
                formData.UserId,
                formData.Amount,
                command.ReviewUserId,
                command.FormId,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
