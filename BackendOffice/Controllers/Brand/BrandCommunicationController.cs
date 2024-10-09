using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandCommunication;
using TGS.Apis.BackendOffice.Filters;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Commands.BrandCommunication;
using TGS.Services.BackOffice.Application.Queries.BrandCommunication;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandCommunicationController : BaseContextController
{
    [BOTypeFilter(typeof(MerchanPermissionBodyFiler), Arguments = ["brand", "BrandId"])]
    [HttpPost]
    [Description("Create a brand communication")]
    public async Task<MessageResponse> CreateBrandCommunicationAsync(
        [FromServices] ICommandService<CreateBrandCommunicationCommand> createService,
        [FromBody] CreateBrandCommunicationRequest request)
    {
        var command = new CreateBrandCommunicationCommand()
        {
            BrandId = request.BrandId,
            CommunicationType = Enum.TryParse<CommunicationType>(request.Type, out var result)
                ? result.ToString()
                : request.Type,
            Phone = request.Phone,
            AppId = request.AppId,
            QRCodeUrl = request.QRCodeUrl
        };
        await createService.ExecuteAsync(command).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [HttpDelete("{id}")]
    [Description("Delete a brand communication")]
    public async Task<MessageResponse> DeleteAsync(
        Guid id,
        [FromServices] ICommandService<DeleteBrandCommunicationCommand> deleteService)
    {
        var command = new DeleteBrandCommunicationCommand(id);
        await deleteService.ExecuteAsync(command).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [BOTypeFilter(typeof(MerchanPermissionRouteFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("{brandId}")]
    [Description("Get a brand's communications")]
    public IAsyncEnumerable<BrandCommunicationModel> GetBrandCommunicationsAsync(
        Guid brandId,
        [FromServices] IQueryService<BrandCommunicationQuery, BrandCommunicationInfo> queryService)
    {
        var query = new BrandCommunicationQuery(brandId);
        var info = queryService.QueryAsync(query);

        return info.Select(x =>
            new BrandCommunicationModel()
            {
                Id = x.Id.ToString(),
                Type = x.CommunicationType,
                Phone = x.Phone,
                AppId = x.AppId,
                QRCodeUrl = x.QRCodeUrl
            });
    }

    [HttpPut("{id}")]
    [Description("Update a brand communication")]
    public async Task<MessageResponse> UpdateBrandCommAsync(
        [FromServices] ICommandService<UpdateBrandCommunicationCommand> updateService,
        Guid id,
        [FromBody] UpdateBrandCommunicationRequest request)
    {
        var command = new UpdateBrandCommunicationCommand()
        {
            Id = id,
            CommunicationType = Enum.TryParse<CommunicationType>(request.Type, out var result)
                ? result.ToString()
                : request.Type,
            Phone = request.Phone,
            AppdId = request.AppId,
            QRCodeUrl = request.QRCodeUrl
        };
        await updateService.ExecuteAsync(command).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }
}