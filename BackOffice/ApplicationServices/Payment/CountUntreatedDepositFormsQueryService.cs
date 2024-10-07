using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;

internal class CountUntreatedDepositFormsQueryService(
    IPaymentServiceClient paymentServiceClient,
    ILogger<CountUntreatedDepositFormsQueryService> logger)
    : IGetService<CountUntreatedDepositFormsQuery, int>
{
    private readonly IPaymentServiceClient _paymentServiceClient = paymentServiceClient ?? throw new ArgumentNullException(nameof(paymentServiceClient));

    public async ValueTask<int> GetAsync(CountUntreatedDepositFormsQuery query, CancellationToken cancellationToken = default)
    {
        var untreatedCount = await _paymentServiceClient
            .CountUntreatedDepositFormsAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("Untreated Deposit Count:{untreatedCount}", untreatedCount);

        return untreatedCount;
    }
}