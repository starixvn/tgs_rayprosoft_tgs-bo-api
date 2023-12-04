using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Banner;

internal class GetBannerDetailQueryService(IBackOfficeRepository backOfficeRepository) : IGetService<GetBannerDetailQuery, BannerInfo>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask<BannerInfo> GetAsync(GetBannerDetailQuery query, CancellationToken cancellationToken = default)
        => _backOfficeRepository.GetBannerByIdAsync(query.BannerId);
}
