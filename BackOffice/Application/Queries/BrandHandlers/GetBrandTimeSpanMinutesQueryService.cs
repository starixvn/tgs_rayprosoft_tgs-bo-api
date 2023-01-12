using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Queries.BrandHandlers;

internal class GetBrandTimeSpanMinutesQueryService : IGetService<GetBrandTimeSpanMinutesQuery, int>
{
    private readonly IBrandService _service;

    public GetBrandTimeSpanMinutesQueryService(IBrandService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<int> GetAsync(
        GetBrandTimeSpanMinutesQuery query,
        CancellationToken cancellationToken = default)
    {
        var brand = await _service.GetBrandQueries()
            .FirstAsync(x => x.Id == query.BrandId, cancellationToken).ConfigureAwait(false);

        return (int)brand.TimeSpanMinutes!;
    }
}
