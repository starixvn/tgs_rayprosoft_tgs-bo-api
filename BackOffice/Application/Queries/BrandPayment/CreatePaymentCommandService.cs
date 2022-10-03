using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.BrandPayment;

internal class CreatePaymentCommandService : ICommandService<CreatePaymentCommand>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public CreatePaymentCommandService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask ExecuteAsync(
        CreatePaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        var createInfo = new PaymentCreateInfo
        {
            Code = command.Code,
            Description = command.Description,
            ExtendedData = command.ExtendedData
        };

        return _paymentServiceClient.CreatePaymentAsync(createInfo);
    }
}
