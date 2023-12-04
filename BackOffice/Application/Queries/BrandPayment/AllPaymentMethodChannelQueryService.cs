using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.BrandPayment;

internal class AllPaymentMethodChannelQueryService(IPaymentServiceClient paymentServiceClient) : IQueryService<AllPaymentMethodChannelQuery, PaymentMethodChannelInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public IAsyncEnumerable<PaymentMethodChannelInfo> QueryAsync(
        AllPaymentMethodChannelQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetAllPaymentMethodChannelsAsync(cancellationToken);
}
