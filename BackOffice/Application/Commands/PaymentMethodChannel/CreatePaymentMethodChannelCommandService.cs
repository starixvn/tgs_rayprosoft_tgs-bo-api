using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Commands.PaymentMethodChannel;

internal class CreatePaymentMethodChannelCommandService : ICommandService<CreatePaymentMethodChannelCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public CreatePaymentMethodChannelCommandService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public async ValueTask ExecuteAsync(CreatePaymentMethodChannelCommand command, CancellationToken cancellationToken = default)
        => await _paymentServiceClient.CreatePaymentMethodChannelAsync(
            command.PaymentId,
            command.MethodId,
            command.ChannelId,
            cancellationToken).ConfigureAwait(false);
}
