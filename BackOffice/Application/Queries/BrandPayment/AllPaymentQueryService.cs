using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.BrandPayment;

internal class AllPaymentQueryService : IQueryService<AllPaymentQuery, PaymentInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public AllPaymentQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public IAsyncEnumerable<PaymentInfo> QueryAsync(
        AllPaymentQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetAllPaymentsAsync();
}
