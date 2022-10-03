using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

internal class SetDefaultPlayerBankInfoCommandService : ICommandService<SetDefaultPlayerBankInfoCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public SetDefaultPlayerBankInfoCommandService(
        IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask ExecuteAsync(SetDefaultPlayerBankInfoCommand command,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient
            .SetDefaultPlayerBankInfoAsync(
                command.PlayerCode,
                command.BankInfoId,
                command.ModifyUser,
                cancellationToken);
}
