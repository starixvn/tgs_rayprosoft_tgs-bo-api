using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services;

internal class CountUntreatedWithdrawFormsQueryService : IGetService<CountUntreatedWithdrawFormsQuery, int>
{
    private readonly IPaymentServiceClient _paymentServiceClient;

    public CountUntreatedWithdrawFormsQueryService(IPaymentServiceClient paymentServiceClient)
    {
        _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));
    }

    public ValueTask<int> GetAsync(
        CountUntreatedWithdrawFormsQuery query,
        CancellationToken cancellationToken = default)
        => _paymentServiceClient.CountUntreatedWithdrawFormsAsync(query.BrandId);
}
