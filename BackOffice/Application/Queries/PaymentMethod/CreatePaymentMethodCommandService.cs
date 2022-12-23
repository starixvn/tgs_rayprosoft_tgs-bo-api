using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PaymentMethod;

internal class CreatePaymentMethodCommandService : ICommandService<CreatePaymentMethodCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public CreatePaymentMethodCommandService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask ExecuteAsync(
        CreatePaymentMethodCommand command,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.CreatePaymentMethodAsync(new PaymentMethodCreateInfo
        {
            Code = command.Code,
            BankType = command.BankType
        }, cancellationToken);
}
