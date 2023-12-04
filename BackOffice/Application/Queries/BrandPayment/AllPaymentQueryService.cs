using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.BrandPayment;

internal class AllPaymentQueryService(IPaymentServiceClient paymentServiceClient) : IQueryService<AllPaymentQuery, PaymentInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public IAsyncEnumerable<PaymentInfo> QueryAsync(
        AllPaymentQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetAllPaymentsAsync(cancellationToken);
}
