using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

public class GetBrandLogoQueryService(IBackOfficeRepository backOfficeRepository) : IGetService<GetBrandLogoQuery, BrandLogoInfo>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public ValueTask<BrandLogoInfo> GetAsync(GetBrandLogoQuery query, CancellationToken cancellationToken = default)
        => _backOfficeRepository.GetBrandLogoInfoAsync(query.BrandId);
}
