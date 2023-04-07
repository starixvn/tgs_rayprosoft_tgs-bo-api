using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Domain;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandService
{
    Task ChangeStatusAsync(Brand brand, UpdateBrandStatusRequest request);

    ValueTask<Guid> CreateBrandAsync(CreateBrandInfo createInfo, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<Brand, bool>> predicate);

    Task<Brand> GetBrandAsync(
        Expression<Func<Brand, bool>> predicate,
        Func<IQueryable<Brand>, IQueryable<Brand>> preQuery = null);

    IQueryable<Brand> GetBrandQueries(
        Func<IQueryable<Brand>, IQueryable<Brand>> preQuery = null,
        bool tracking = false);

    Task SyncBrandGamesAsync(Guid brandId);

    Task UpdateBrandAsync(Brand brand, UpdateBrandRequest request);
}