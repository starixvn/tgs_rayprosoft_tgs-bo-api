using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetBannersQueryService : IQueryService<GetBannersQuery, BannerInfo>, IGetService<GetBannersQuery, int>
{
    private readonly IBackOfficeRepository _backOfficeRepository;

    public GetBannersQueryService(IBackOfficeRepository backOfficeRepository)
    {
        _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));
    }

    public ValueTask<int> GetAsync(GetBannersQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);

        return data.CountAsync(cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<BannerInfo> QueryAsync(GetBannersQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);

        return data.ApplyOrderBy(query.Paging).ApplyPaging(query.Paging);
    }

    private IAsyncEnumerable<BannerInfo> MakeDataSourceAsync(GetBannersQuery query)
    {
        return query.BannerStatus switch
        {
            BannerStatus.Reviewing => _backOfficeRepository.QueryReviewingBannersAsync(query.BrandId),
            BannerStatus.Valid => _backOfficeRepository.QueryValidBannersAsync(query.BrandId),
            BannerStatus.Expired => _backOfficeRepository.QueryExpiredBannersAsync(query.BrandId),
            BannerStatus.Rejected => _backOfficeRepository.QueryRejectedBannersAsync(query.BrandId),
            _ => null
        };
    }
}
