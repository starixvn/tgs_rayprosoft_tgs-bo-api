using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.ApplicationServices.Players;

internal class GetPlayerIdentityAuthenticationListQueryService(IBackOfficeRepository backOfficeRepository) :
    IQueryService<GetPlayerIdentityAuthenticationFormsQuery, PlayerIdentityAuthenticationInfo>,
    IGetService<GetPlayerIdentityAuthenticationFormsQuery, int>
{
    private readonly IBackOfficeRepository _backOfficeRepository = backOfficeRepository ?? throw new ArgumentNullException(nameof(backOfficeRepository));

    public async ValueTask<int> GetAsync(GetPlayerIdentityAuthenticationFormsQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);

        return await data.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public IAsyncEnumerable<PlayerIdentityAuthenticationInfo> QueryAsync(
        GetPlayerIdentityAuthenticationFormsQuery query,
        CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);

        return data.ApplyOrderBy(query.Paging).ApplyPaging(query.Paging);
    }

    private IAsyncEnumerable<PlayerIdentityAuthenticationInfo> MakeDataSourceAsync(GetPlayerIdentityAuthenticationFormsQuery query)
        => _backOfficeRepository.QueryPlayerIdentityAuthenticationsAsync(
            query.BrandId,
            query.Status,
            query.Username,
            query.BeginCreatedOn,
            query.EndCreatedOn,
            query.BeginReviewedOn,
            query.EndReviewedOn);
}
