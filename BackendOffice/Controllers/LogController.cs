using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.Log;
using TGS.Apis.BackendOffice.DataService.Repositories;
using TGS.Core.DataServices.Base.Extensions;
using TGS.Core.Models.Messages;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogController : BaseContextController
{
    private readonly ILogQueries _logQueries;

    public LogController(
        ILogQueries logQueries)
        => _logQueries = logQueries;

    [HttpGet("admin/{id:Guid}")]
    [ByPassAdminLog]
    public async Task<LogDetailResponse> GetAdminLogAsync(Guid id)
    {
        var logDetail = await _logQueries.GetAdminLogAsync(id);

        return new LogDetailResponse
        {
            Valid = true,
            LogDetail = logDetail
        };
    }

    [HttpGet("admin")]
    [ByPassAdminLog]
    public async Task<PagedResponse<AdminLogModel>> GetAdminLogsAsync([FromQuery] AdminLogRequest request)
    {
        var (logs, totalCount) = await _logQueries.GetAdminLogsAsync(request);

        return await logs.CreatePagedResponseAsync(totalCount);
    }

    [HttpGet("api/{id:Guid}")]
    [ByPassAdminLog]
    public async Task<LogDetailResponse> GetApiLogAsync(Guid id)
    {
        var logDetail = await _logQueries.GetApiLogAsync(id);

        return new LogDetailResponse
        {
            Valid = true,
            LogDetail = logDetail
        };
    }

    [HttpGet("api")]
    [ByPassAdminLog]
    public async Task<PagedResponse<ApiLogModel>> GetApiLogsAsync([FromQuery] ApiLogRequest request)
    {
        var (logs, totalCount) = await _logQueries.GetApiLogsAsync(request);

        return await logs.CreatePagedResponseAsync(totalCount);
    }

    [HttpGet("log")]
    [ByPassAdminLog]
    public async Task<PagedResponse<LogRecordModel>> GetLogRecordsAsync([FromQuery] LogRecordRequest request)
    {
        var (logs, totalCount) = await _logQueries.GetLogRecordsAsync(request);

        return await logs.CreatePagedResponseAsync(totalCount);
    }
}
