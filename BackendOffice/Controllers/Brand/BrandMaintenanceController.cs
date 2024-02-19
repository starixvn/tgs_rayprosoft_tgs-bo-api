using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandMaintenance;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandMaintenanceController : BaseContextController
{
    [HttpGet("{brandMaintenanceId}", Name = "GetBrandMaintenance")]
    [Description("Get a brand maintenance content")]
    public ValueTask<BrandMaintenanceResult> GetBrandMaintenanceAsync(
        Guid brandMaintenanceId,
        [FromServices] IBrandServiceClient brandServiceClient)
        => brandServiceClient.GetBrandMaintenanceInfoByIdAsync(brandMaintenanceId);

    [HttpGet(Name = "GetBrandMaintenanceInfos")]
    [Description("Get all brand maintenance infos")]
    public async IAsyncEnumerable<TGS.Services.BackOffice.Domain.BrandMaintenanceInfo> GetBrandMaintenanceInfosAsync(
        [FromServices] IBrandServiceClient brandServiceClient,
        [FromQuery] GetBrandMaintenanceInfosRequest request)
    {
        var infos = await brandServiceClient
            .GetBrandMaintenanceInfosAsync(request.BrandCode, HttpContext.RequestAborted)
            .ToArrayAsync(HttpContext.RequestAborted)
            .ConfigureAwait(false);

        foreach (var info in infos)
        {
            var beginTime = string.IsNullOrEmpty(info.BeginTime)
                ? null
                : TimeOnly.Parse(info.BeginTime).ToString("HH:mm");

            var endTime = string.IsNullOrEmpty(info.EndTime)
                ? null
                : TimeOnly.Parse(info.EndTime).ToString("HH:mm");

            yield return new TGS.Services.BackOffice.Domain.BrandMaintenanceInfo
            (
                info.BrandName,
                info.BrandCode,
                info.LicenseeName,
                info.BrandId,
                info.BrandStatus,
                beginTime,
                endTime,
                info.BrandMaintenanceId
            );
        }
    }

    [HttpPut("allmaintain")]
    [Description("Change all open status brands to maintenance status")]
    public async Task<MessageResponse> UpdateAllBrandMaintenanceStatusAsync(
        [FromServices] ICommandService<UpdateAllBrandMaintenanceStatusCommand> commandService)
    {
        await commandService
            .ExecuteAsync(new UpdateAllBrandMaintenanceStatusCommand(Username))
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("updatealltime", Name = "UpdateAllBrandMaintenanceTime")]
    [Description("Update all brand maintenance")]
    public async Task<MessageResponse> UpdateAllBrandMaintenanceTimeAsync(
        [FromBody] UpdateAllBrandMaintenanceTimeRequest request,
        [FromServices] IBrandServiceClient brandServiceClient,
        [FromServices] ICommandService<ClearAllBrandMaintenanceCacheCommand> commandService)
    {
        await brandServiceClient
            .UpdateAllBrandMaintenanceTimeAsync(request.BeginTime, request.EndTime)
            .ConfigureAwait(false);

        await commandService.ExecuteAsync(new ClearAllBrandMaintenanceCacheCommand());

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("content/{brandMaintenanceId}", Name = "UpdateBrandMaintenanceContent")]
    [Description("Update a brand maintenance's content")]
    public async Task<MessageResponse> UpdateBrandMaintenanceContentAsync(
        Guid brandMaintenanceId,
        [FromBody] UpdateBrandMaintenanceContentRequest request,
        [FromServices] IBrandServiceClient brandServiceClient)
    {
        await brandServiceClient
            .UpdateBrandMaintenanceContentAsync(brandMaintenanceId, request.Content)
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpPut("time/{brandId}", Name = "UpdateBrandMaintenanceTime")]
    [Description("Update a brand maintenance's begin time and end time")]
    public async Task<MessageResponse> UpdateBrandMaintenanceTimeAsync(
        Guid brandId,
        [FromBody] UpdateBrandMaintenanceTimeRequest request,
        [FromServices] IBrandServiceClient brandServiceClient,
        [FromServices] ICommandService<ClearBrandMaintenanceCacheCommand> commandService)
    {
        await brandServiceClient
            .UpdateBrandMaintenanceTimeAsync(brandId, request.BeginTime, request.EndTime)
            .ConfigureAwait(false);

        await commandService
            .ExecuteAsync(new ClearBrandMaintenanceCacheCommand(brandId))
            .ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }
}
