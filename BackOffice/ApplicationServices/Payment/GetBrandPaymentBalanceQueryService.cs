#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.Payment;
internal class GetBrandPaymentBalanceQueryService(
    IPaymentServiceClient paymentServiceClient,
    IPaymentClient paymentClient)
    : IGetService<GetBrandPaymentBalanceQuery, decimal?>
{
    public async ValueTask<decimal?> GetAsync(GetBrandPaymentBalanceQuery query, CancellationToken cancellationToken = default)
    {
        var brandPayment = await paymentServiceClient.GetBrandPaymentByIdAsync(
            query.BrandPaymentId,
            cancellationToken).ConfigureAwait(false);

        if (brandPayment is null || brandPayment.BrandId != query.BrandId || string.IsNullOrEmpty(brandPayment.ExtendedData))
            return null;

        var paymentInfo = await paymentServiceClient.GetPaymentByIdAsync(
            brandPayment.PaymentId,
            cancellationToken).ConfigureAwait(false);

        return paymentInfo is null
            ? null
            : await paymentClient.GetMerchantBalanceAsync(
                paymentInfo.Code,
                brandPayment.ExtendedData,
                cancellationToken).ConfigureAwait(false);
    }
}
