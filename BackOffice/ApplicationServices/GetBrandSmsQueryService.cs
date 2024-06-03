using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetBrandSmsQueryService(IBrandServiceClient brandServiceClient)
    : IGetService<GetBrandSmsQuery, BrandSmsResult>
{
    public async ValueTask<BrandSmsResult> GetAsync(GetBrandSmsQuery query, CancellationToken cancellationToken = default)
    {
        var brand = await brandServiceClient
            .GetBrandInfoAsync(query.BrandId, cancellationToken)
            .ConfigureAwait(false);

        if (brand.SmsEnabled)
        {
            var smsInfo = await brandServiceClient
                .GetBrandSmsSettingsAsync<JsonDocument>(query.BrandId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new BrandSmsResult(brand.SmsEnabled, smsInfo.Code);
        }

        return new BrandSmsResult(brand.SmsEnabled, null);
    }
}