using System;
using System.Threading;
using System.Threading.Tasks;
using TGS.Services.BackOffice.Domain;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.Application.Services;

public class GetBrandLogoQueryService : IGetService<GetBrandLogoQuery, BrandLogoInfo>
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    public GetBrandLogoQueryService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public ValueTask<BrandLogoInfo> GetAsync(GetBrandLogoQuery query, CancellationToken cancellationToken = default)
        => _backOfficeRepository.GetBrandLogoInfoAsync(query.BrandId);
}
