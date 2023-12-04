using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tgs.Logging;
using TGS.Apis.BackendOffice.Controllers.Messages.Log;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.DataServices.Tgs;

namespace TGS.Apis.BackendOffice.DataService.Repositories;

public class LogQueries(ITgsRepository repository) : ILogQueries
{
    Task<bool> ILogQueries.ExistsAsync<T>(Expression<Func<T, bool>> predicate) =>
        repository.Set<T>().AnyAsync(predicate);

    Task<LogDetailModel> ILogQueries.GetAdminLogAsync(Guid trackingId)
    {
        const string spName = "log.log_get_admin_log";
        return repository.SqlQuerySingleAsync<LogDetailModel>(spName,
            parameters: new Dictionary<string, object> { { "_tracking_id", trackingId } });
    }

    async Task<(IEnumerable<AdminLogModel>, int)> ILogQueries.GetAdminLogsAsync(AdminLogRequest request)
    {
        const string spName = "log.log_get_admin_logs";
        var results = await repository.SqlQueryMultipleAsync<AdminLogModel>(spName,
            parameters: new Dictionary<string, object>
            {
                {"_from", request.From},
                {"_to", request.To},
                {"_tracking_id", request.TrackingId},
                {"_ip", request.Ip},
                {"_path", request.Path},
                {"_method", request.Method},
                {"_server_name", request.ServerName},
                {"_response_code", request.ResponseCode},
                {"_page_size", request.ShowAll ? int.MaxValue : request.Rows},
                {"_page_number", request.ShowAll ? 1 : request.Page},
                {"_sort_by", request.SortBy},
                {"_is_desc", request.IsDesc}
            });
        return (results, results.Any() ? results.First().TotalCount : 0);
    }

    Task<LogDetailModel> ILogQueries.GetApiLogAsync(Guid trackingId)
    {
        const string spName = "log.log_get_api_log";
        return repository.SqlQuerySingleAsync<LogDetailModel>(spName,
            parameters: new Dictionary<string, object> { { "_tracking_id", trackingId } });
    }

    async Task<(IEnumerable<ApiLogModel>, int)> ILogQueries.GetApiLogsAsync(ApiLogRequest request)
    {
        const string spName = "log.log_get_api_logs";
        var results = await repository.SqlQueryMultipleAsync<ApiLogModel>(spName,
            parameters: new Dictionary<string, object>
            {
                {"_from", request.From},
                {"_to", request.To},
                {"_tracking_id", request.TrackingId},
                {"_ip", request.Ip},
                {"_path", request.Path},
                {"_method", request.Method},
                {"_prefix", request.Prefix},
                {"_server_name", request.ServerName},
                {"_response_code", request.ResponseCode},
                {"_page_size", request.ShowAll ? int.MaxValue : request.Rows},
                {"_page_number", request.ShowAll ? 1 : request.Page},
                {"_sort_by", request.SortBy},
                {"_is_desc", request.IsDesc}
            });
        return (results, results.Any() ? results.First().TotalCount : 0);
    }

    async Task<(IEnumerable<LogRecordModel>, int)> ILogQueries.GetLogRecordsAsync(LogRecordRequest request)
    {
        const string spName = "log.log_get_log_records";
        var results = await repository.SqlQueryMultipleAsync<LogRecordModel>(spName,
            parameters: new Dictionary<string, object>
            {
                {"_from", request.From},
                {"_to", request.To},
                {"_tracking_id", request.TrackingId},
                {"_server_name", request.ServerName},
                {"_level", request.Level == TgsLogLevel.All ? null : request.Level},
                {"_page_size", request.ShowAll ? int.MaxValue : request.Rows},
                {"_page_number", request.ShowAll ? 1 : request.Page},
                {"_sort_by", request.SortBy},
                {"_is_desc", request.IsDesc}
            });
        return (results, results.Any() ? results.First().TotalCount : 0);
    }

    IQueryable<T> ILogQueries.GetQueries<T>() => repository.Set<T>().AsQueryable();

    Task<T> ILogQueries.GetSingleAsync<T>(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>> preQuery)
    {
        preQuery ??= c => c;
        return preQuery(repository.Set<T>()).FirstOrDefaultAsync(predicate);
    }
}
