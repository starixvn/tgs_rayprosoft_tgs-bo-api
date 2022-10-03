using System;
using System.Collections.Generic;
using System.Threading;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Queries.PaymentChannel;

internal class PaymentChannelQueryService : IQueryService<PaymentChannelQuery, PaymentChannelInfo>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public PaymentChannelQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public IAsyncEnumerable<PaymentChannelInfo> QueryAsync(
        PaymentChannelQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.GetAllPaymentChannelsAsync();
}
