using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.PlayerBucket;

namespace TGS.Services.BackOffice.Application.Commands.Withdraw;

internal class CreateWithdrawFormCommandService : ICommandService<CreateWithdrawFormCommand>
{
    private readonly ICashServiceClient _cashServiceClient;
    private readonly IPaymentServiceClient _paymentServiceClient;
    private readonly IPlayerBucketRepository _playerBucketRepository;
    private readonly IPlayerServiceClient _playerServiceClient;

    public CreateWithdrawFormCommandService(
        IPaymentServiceClient paymentServiceClient,
        ICashServiceClient cashServiceClient,
        IPlayerServiceClient playerServiceClient,
        IPlayerBucketRepository playerBucketRepository)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
        _cashServiceClient = cashServiceClient ?? throw new ArgumentNullException(nameof(cashServiceClient));
        _playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
        _playerBucketRepository = playerBucketRepository ?? throw new ArgumentNullException(nameof(playerBucketRepository));
    }

    public async ValueTask ExecuteAsync(CreateWithdrawFormCommand command, CancellationToken cancellationToken = default)
    {
        var playerInfo = await _playerServiceClient
            .GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new Exception("Can't find player.");

        var bucketInfo = await _playerBucketRepository.GetPlayerBucketInfoAsync(
            command.PlayerCode,
            cancellationToken).ConfigureAwait(false);

        if (bucketInfo.Amount > 0M)
            throw new Exception("InsufficientBucket");

        var playerProps = await _playerServiceClient
            .GetPlayerPropertiesAsync(command.PlayerCode, cancellationToken)
            .ConfigureAwait(false);

        var playerWalletId = Guid.Parse(playerProps["DefaultWalletId"]!);

        var balance = await _cashServiceClient
            .GetBalanceAsync(playerWalletId, cancellationToken)
            .ConfigureAwait(false);

        if (balance - command.Amount < 0)
            throw new Exception("InsufficientFunds");

        var createFormId = await _paymentServiceClient
            .CreateWithdrawFormAsync(
                new WithdrawFormCreateInfo
                {
                    Amount = command.Amount,
                    BrandId = command.BrandId,
                    BankInfoId = command.BankInfoId,
                    UserId = command.PlayerCode,
                    UserName = playerInfo.UserName,
                    UserType = UserType.Player,
                    Note = command.Note,
                },
                cancellationToken)
            .ConfigureAwait(false);

        var result = await _cashServiceClient
            .LockWithdrawAsync(
                playerWalletId,
                command.Amount,
                null,
                null,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.Balance < 0)
        {
            _ = await _cashServiceClient.RollbackWithdrawAsync(
                playerWalletId,
                command.Amount,
                null,
                null,
                null,
                cancellationToken).ConfigureAwait(false);

            await _paymentServiceClient
                .SubmitWithdrawFormStatusAsync(createFormId, PaymentFlowState.Reject, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            command.ReturnFormIdSource.SetResult(createFormId);

            throw new Exception("InsufficientFunds");
        }

        await _paymentServiceClient
            .SubmitWithdrawFormStatusAsync(
                createFormId,
                PaymentFlowState.WaitApprove,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        command.ReturnFormIdSource.SetResult(createFormId);
    }
}
