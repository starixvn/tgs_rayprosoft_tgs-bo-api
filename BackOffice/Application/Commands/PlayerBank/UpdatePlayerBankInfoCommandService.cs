using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

internal class UpdatePlayerBankInfoCommandService(
    IPaymentServiceClient paymentServiceClient) : ICommandService<UpdatePlayerBankInfoCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public async ValueTask ExecuteAsync(
        UpdatePlayerBankInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _paymentServiceClient
            .DeletePlayerBankInfoAsync(command.PlayerCode, command.BankInfoId, cancellationToken)
            .ConfigureAwait(false);

        var createInfo = new BankCreateInfo
        {
            BankType = command.BankType,
            PaymentChannelId = command.PaymentChannelId,
            Detail = command.Detail,
            IsDefault = false
        };

        _ = await _paymentServiceClient
            .CreatePlayerBankInfoAsync(
                command.BrandId,
                command.PlayerCode,
                createInfo,
                command.ModifyUser,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
