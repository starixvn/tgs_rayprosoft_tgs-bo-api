using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class CountUntreatedDepositFormsQueryService(IPaymentServiceClient paymentServiceClient) : IGetService<CountUntreatedDepositFormsQuery, int>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public ValueTask<int> GetAsync(CountUntreatedDepositFormsQuery query, CancellationToken cancellationToken = default)
        => _paymentServiceClient.CountUntreatedDepositFormsAsync(query.BrandId, cancellationToken);
}
