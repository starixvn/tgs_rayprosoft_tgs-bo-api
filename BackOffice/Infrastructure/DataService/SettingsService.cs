using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Infrastructure.DataService;

public interface ISettingsService
{
    Task<Setting> GetSettingAsync(Expression<Func<Setting, bool>> predicate,
        Func<IQueryable<Setting>, IQueryable<Setting>> preQuery = null);
}

public class SettingsService : BaseRepositoryOperations<ITgsRepository>, ISettingsService
{
    public SettingsService(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    Task<Setting> ISettingsService.GetSettingAsync(Expression<Func<Setting, bool>> predicate,
        Func<IQueryable<Setting>, IQueryable<Setting>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);
}
