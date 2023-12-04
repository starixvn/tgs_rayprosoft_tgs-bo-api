using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PlayerBank;

internal class DeletePlayerBankInfoCommandService(
    IPaymentServiceClient paymentServiceClient) : ICommandService<DeletePlayerBankInfoCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public ValueTask ExecuteAsync(DeletePlayerBankInfoCommand command,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient
            .DeletePlayerBankInfoAsync(
                command.PlayerCode,
                command.BankInfoId,
                cancellationToken);
}
