using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tgs.Paging;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Base.Interfaces;
using Department = TGS.Core.DataServices.Tgs.Models.Department;

namespace TGS.Services.BackOffice.ApplicationServices;

internal class GetDepartmentsQueryService : IQueryService<GetDepartmentsQuery, DepartmentInfo>,
    IGetService<GetDepartmentsQuery, int>
{
    private readonly ITgsQueries<Department> _queries;

    public GetDepartmentsQueryService(
        ITgsQueries<Department> queries)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    }

    public ValueTask<int> GetAsync(GetDepartmentsQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);
        return data.CountAsync(cancellationToken);
    }

    public IAsyncEnumerable<DepartmentInfo> QueryAsync(GetDepartmentsQuery query, CancellationToken cancellationToken = default)
    {
        var data = MakeDataSourceAsync(query);

        return data.ApplyOrderBy(query.Paging).ApplyPaging(query.Paging);
    }

    private IAsyncEnumerable<DepartmentInfo> MakeDataSourceAsync(GetDepartmentsQuery query)
    {
        var departmentsQuery = _queries.GetQueries()
            .Where(query.ResellerId != null, r => r.ResellerId == query.ResellerId)
            .Where(query.LicenseeId != null, l => l.LicenseeId == query.LicenseeId)
            .Where(query.BrandId != null, l => l.BrandId == query.BrandId)
            .Where(query.DepartmentName, d => d.Name == query.DepartmentName)
            .Select(d => new DepartmentInfo(
                d.Id,
                d.ResellerId,
                d.LicenseeId,
                d.BrandId,
                d.Reseller.Name,
                d.Licensee.Name,
                d.Brand.Name,
                d.Name));

        return departmentsQuery.ToAsyncEnumerable();
    }
}
