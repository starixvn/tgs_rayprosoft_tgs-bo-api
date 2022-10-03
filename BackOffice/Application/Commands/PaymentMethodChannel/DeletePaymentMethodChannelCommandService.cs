using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PaymentMethodChannel;

internal class DeletePaymentMethodChannelCommandService : ICommandService<DeletePaymentMethodChannelCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public DeletePaymentMethodChannelCommandService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask ExecuteAsync(DeletePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
        => _paymentServiceClient.DeletePaymentMethodChannelAsync(command.Id, cancellationToken);
}
