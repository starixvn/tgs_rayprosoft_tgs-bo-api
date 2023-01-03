using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PaymentChannel;

internal class CreatePaymentChannelCommandService : ICommandService<CreatePaymentChannelCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public CreatePaymentChannelCommandService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask ExecuteAsync(
        CreatePaymentChannelCommand command,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.CreatePaymentChannelAsync(command.Code, cancellationToken);
}
