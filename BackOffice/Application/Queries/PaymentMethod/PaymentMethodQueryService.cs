using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PaymentMethod;

internal class PaymentMethodQueryService(IPaymentServiceClient paymentServiceClient) : IQueryService<PaymentMethodQuery, PaymentMethodInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public IAsyncEnumerable<PaymentMethodInfo> QueryAsync(PaymentMethodQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetAllPaymentMethodsAsync(cancellationToken);
}
