using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class CountUntreatedWithdrawFormsQueryService(
    IPaymentServiceClient paymentServiceClient,
    ILogger<CountUntreatedWithdrawFormsQueryService> logger)
    : IGetService<CountUntreatedWithdrawFormsQuery, int>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public async ValueTask<int> GetAsync(
        CountUntreatedWithdrawFormsQuery query,
        CancellationToken cancellationToken = default)
    {
        var untreatedCount = await _paymentServiceClient
            .CountUntreatedWithdrawFormsAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Untreated Withdraw Count:{untreatedCount}", untreatedCount);

        return untreatedCount;
    }
}