using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PaymentMethodChannel;

internal class DeletePaymentMethodChannelCommandService(IPaymentServiceClient paymentServiceClient) : ICommandService<DeletePaymentMethodChannelCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public ValueTask ExecuteAsync(DeletePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
        => _paymentServiceClient.DeletePaymentMethodChannelAsync(command.Id, cancellationToken);
}
