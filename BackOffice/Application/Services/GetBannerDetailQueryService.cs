using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Application.Services;

internal class GetBannerDetailQueryService : IGetService<GetBannerDetailQuery, BannerInfo>
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    public GetBannerDetailQueryService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public ValueTask<BannerInfo> GetAsync(GetBannerDetailQuery query, CancellationToken cancellationToken = default)
        => _backOfficeRepository.GetBannerByIdAsync(query.BannerId);
}
