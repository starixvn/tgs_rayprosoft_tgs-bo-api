using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

internal class PaidWithdrawFormCommandService(
    IPaymentServiceClient paymentServiceClient,
    IWalletExchangeService walletExchangeService) : ICommandService<PaidWithdrawFormCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    private readonly IWalletExchangeService _walletExchangeService = walletExchangeService ?? throw new ArgumentNullException(nameof(walletExchangeService));

    public async ValueTask ExecuteAsync(PaidWithdrawFormCommand command, CancellationToken cancellationToken = default)
    {
        var formData = await _paymentServiceClient
            .GetWithdrawFormAsync(command.BrandId, command.FormId, cancellationToken)
            .ConfigureAwait(false);

        var fsm = _paymentServiceClient.CreateWithdrawStateMachine(formData.Status);

        if (!fsm.CanFire(PaymentTrigger.PaymentWithdrawCompleted))
            return;

        await fsm
            .FireAsync(PaymentTrigger.PaymentWithdrawCompleted)
            .ConfigureAwait(false);

        await _paymentServiceClient
            .SubmitWithdrawFormStatusAsync(
                formData.Id,
                fsm.State,
                paidUserId: command.PaidUserId,
                paidUserName: command.PaidUserName,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        await _walletExchangeService.CommitWithdrawAsync(
            formData.BrandId,
            formData.UserType,
            formData.UserId,
            formData.Amount,
            formData.Amount,
            string.IsNullOrWhiteSpace(command.PaidUserId)
                ? Guid.Empty.ToString()
                : command.PaidUserId,
            formData.Id,
            cancellationToken).ConfigureAwait(false);
    }
}
