using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TGS.Apis.BackendOffice.Controllers.Messages.Log;

namespace TGS.Apis.BackendOffice.DataService.Repositories;

public interface ILogQueries
{
    Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

    Task<LogDetailModel> GetAdminLogAsync(Guid trackingId);

    Task<(IEnumerable<AdminLogModel>, int)> GetAdminLogsAsync(AdminLogRequest request);

    Task<LogDetailModel> GetApiLogAsync(Guid trackingId);

    Task<(IEnumerable<ApiLogModel>, int)> GetApiLogsAsync(ApiLogRequest request);

    Task<(IEnumerable<LogRecordModel>, int)> GetLogRecordsAsync(LogRecordRequest request);

    IQueryable<T> GetQueries<T>() where T : class;

    Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>> preQuery = null) where T : class;
}
