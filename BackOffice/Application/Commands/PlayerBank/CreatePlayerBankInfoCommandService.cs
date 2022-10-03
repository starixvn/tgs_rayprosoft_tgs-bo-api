using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

internal class CreatePlayerBankInfoCommandService : ICommandService<CreatePlayerBankInfoCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public CreatePlayerBankInfoCommandService(
        IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public async ValueTask ExecuteAsync(
        CreatePlayerBankInfoCommand command,
        CancellationToken cancellationToken = default)
    {
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
