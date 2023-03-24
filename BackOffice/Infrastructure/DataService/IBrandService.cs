using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Application.Models.Brand;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface IBrandService
{
    Task ChangeStatusAsync(Brand brand, UpdateBrandStatusRequest request);

    Task CreateBrandAsync(CreateBrandRequest request);

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