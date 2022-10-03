using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Core.DataServices.Tgs.Models;

namespace TGS.Services.BackOffice.Domain;

public interface ISettingRepository
{
    Task<Setting> GetSettingAsync(Expression<Func<Setting, bool>> predicate,
        Func<IQueryable<Setting>, IQueryable<Setting>> preQuery = null);
}