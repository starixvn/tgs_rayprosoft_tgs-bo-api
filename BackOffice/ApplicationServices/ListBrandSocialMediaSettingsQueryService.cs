using System.Collections.Generic;
using System.Threading;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class ListBrandSocialMediaSettingsQueryService(IBrandServiceClient brandServiceClient)
    : IQueryService<ListBrandSocialMediaSettingsQuery, SocialMediaSetting>
{
    public IAsyncEnumerable<SocialMediaSetting> QueryAsync(
        ListBrandSocialMediaSettingsQuery query,
        CancellationToken cancellationToken = default)
        => brandServiceClient.ListSocialMediaSettingsAsync(query.BrandId, cancellationToken);
}
