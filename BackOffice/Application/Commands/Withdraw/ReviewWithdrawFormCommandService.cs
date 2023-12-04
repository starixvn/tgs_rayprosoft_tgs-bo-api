using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

internal class ReviewWithdrawFormCommandService(
    IPaymentServiceClient paymentServiceClient,
    IWalletExchangeService walletExchangeService) : ICommandService<ReviewWithdrawFormCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    private readonly IWalletExchangeService _walletExchangeService = walletExchangeService ?? throw new ArgumentNullException(nameof(walletExchangeService));

    public async ValueTask ExecuteAsync(
        ReviewWithdrawFormCommand command, CancellationToken cancellationToken = default)
    {
        var formData = await _paymentServiceClient
            .GetWithdrawFormAsync(command.BrandId, command.FormId, cancellationToken)
            .ConfigureAwait(false);

        var fsm = _paymentServiceClient.CreateWithdrawStateMachine(formData.Status);

        if (command.Status == ReviewState.Approve)
        {
            if (!fsm.CanFire(PaymentTrigger.Approve))
                return;

            await _paymentServiceClient
                .ReviewWithdrawFormAsync(
                    command.FormId,
                    command.BrandPaymentId.Value,
                    command.Status,
                    command.ReviewUserId,
                    command.ReviewUserName,
                    command.Remark,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            if (!fsm.CanFire(PaymentTrigger.Reject))
                return;

            await _paymentServiceClient
                .ReviewWithdrawFormAsync(
                    command.FormId,
                    command.BrandPaymentId,
                    command.Status,
                    command.ReviewUserId,
                    command.ReviewUserName,
                    command.Remark,
                    cancellationToken)
                .ConfigureAwait(false);

            await _walletExchangeService.RollbackWithdrawAsync(
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
