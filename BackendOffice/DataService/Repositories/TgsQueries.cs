using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs;

namespace TGS.Apis.BackendOffice.DataService.Repositories;

public class TgsQueries<T>(ITgsRepository repository) : ITgsQueries<T> where T : class
{
    Task<bool> ITgsQueries<T>.ExistsAsync(Expression<Func<T, bool>> predicate) =>
        repository.Set<T>().AnyAsync(predicate);

    Task<int> ITgsQueries<T>.GetCountAsync(Expression<Func<T, bool>> predicate) =>
        repository.Set<T>().CountAsync(predicate);

    IQueryable<T> ITgsQueries<T>.GetQueries() => repository.Set<T>();

    Task<T> ITgsQueries<T>.GetSingleAsync(Expression<Func<T, bool>> predicate,
                Func<IQueryable<T>, IQueryable<T>> preQuery)
    {
        preQuery ??= c => c;
        return preQuery(repository.Set<T>()).FirstOrDefaultAsync(predicate);
    }
}
