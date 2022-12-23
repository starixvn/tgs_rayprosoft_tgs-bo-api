using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Base;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Services.BackOffice.Domain;

namespace TGS.BackOffice.Repositories;

public class SettingRepository : BaseRepositoryOperations<ITgsRepository>, ISettingRepository
{
    public SettingRepository(ITgsRepository tgsRepository) : base(
        tgsRepository)
    {
    }

    Task<Setting> ISettingRepository.GetSettingAsync(Expression<Func<Setting, bool>> predicate,
        Func<IQueryable<Setting>, IQueryable<Setting>> preQuery) =>
        GetEntityFirstOrDefaultAsync(predicate, preQuery);
}
