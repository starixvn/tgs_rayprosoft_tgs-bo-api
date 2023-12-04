using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.DataServices.Tgs;

namespace TGS.Apis.BackendOffice.DataService.Repositories;

public class TgsCommands<TEntity>(ITgsRepository repository) : ITgsCommands<TEntity> where TEntity : class
{
    async Task ITgsCommands<TEntity>.BatchUpdateAsync(TEntity[] obj)
    {
        repository.Set<TEntity>().UpdateRange(obj);
        _ = await repository.SaveChangesAsync();
    }

    Task ITgsCommands<TEntity>.CreateAsync(TEntity obj)
    {
        repository.Entry(obj).State = EntityState.Added;
        return repository.SaveChangesAsync();
    }

    Task ITgsCommands<TEntity>.DeleteAsync(TEntity obj)
    {
        repository.Entry(obj).State = EntityState.Deleted;
        return repository.SaveChangesAsync();
    }

    Task ITgsCommands<TEntity>.UpdateAsync(TEntity obj)
    {
        repository.Entry(obj).State = EntityState.Modified;
        return repository.SaveChangesAsync();
    }
}
