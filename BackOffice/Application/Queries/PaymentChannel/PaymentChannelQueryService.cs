using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PaymentChannel;

internal class PaymentChannelQueryService(IPaymentServiceClient paymentServiceClient) : IQueryService<PaymentChannelQuery, PaymentChannelInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public IAsyncEnumerable<PaymentChannelInfo> QueryAsync(
        PaymentChannelQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetAllPaymentChannelsAsync(cancellationToken);
}
