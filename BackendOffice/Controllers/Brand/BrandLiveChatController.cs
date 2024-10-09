using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandLiveChat;
using TGS.Apis.BackendOffice.Filters;
using TGS.Services.BackOffice.Application.Commands.BrandLiveChat;
using TGS.Services.BackOffice.Application.Queries.BrandLiveChat;
using TGS.Services.BackOffice.Domain;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/[controller]")]
[ApiController]
public class BrandLiveChatController : BaseContextController
{
    [BOTypeFilter(typeof(MerchanPermissionBodyFiler), Arguments = ["brand", "BrandId"])]
    [HttpPost]
    [Description("Create a brand live chat")]
    public async Task<MessageResponse> CreateBrandCommunicationAsync(
        [FromServices] ICommandService<CreateBrandLiveChatCommand> createService,
        [FromBody] CreateBrandLiveChatRequest request)
    {
        var command = new CreateBrandLiveChatCommand()
        {
            BrandId = request.BrandId,
            Name = request.Name,
            Setting = JsonDocument.Parse(request.Setting)
        };
        await createService.ExecuteAsync(command).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [BOTypeFilter(typeof(MerchanPermissionQueryFiler), Arguments = ["brand", "brandId"])]
    [HttpDelete("{brandId}")]
    [Description("Delete a brand live chat")]
    public async Task<MessageResponse> DeleteAsync(
        Guid brandId,
        [FromServices] ICommandService<DeleteBrandLiveChatCommand> deleteService)
    {
        var command = new DeleteBrandLiveChatCommand(brandId);
        await deleteService.ExecuteAsync(command).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }

    [BOTypeFilter(typeof(MerchanPermissionQueryFiler), Arguments = ["brand", "brandId"])]
    [HttpGet("{brandId}")]
    [Description("Get a brand's live chat")]
    public async Task<GetBrandLiveChatResponse> GetBrandLiveChatAsync(
        Guid brandId,
        [FromServices] IGetService<BrandLiveChatQuery, BrandLiveChatInfo> queryService)
    {
        var query = new BrandLiveChatQuery(brandId);
        var info = await queryService.GetAsync(query).ConfigureAwait(false);

        return info == null
            ? new GetBrandLiveChatResponse()
            : new GetBrandLiveChatResponse()
            {
                Data = new BrandLiveChatModel()
                {
                    Name = info.ChatName,
                    Setting = info.ChatSetting
                }
            };
    }

    [BOTypeFilter(typeof(MerchanPermissionQueryFiler), Arguments = ["brand", "brandId"])]
    [HttpPut("{brandId}")]
    [Description("Update a brand live chat")]
    public async Task<MessageResponse> UpdateBrandCommAsync(
        Guid brandId,
        [FromBody] UpdateBrandLiveChatRequest request,
        [FromServices] ICommandService<UpdateBrandLiveChatCommand> updateService)
    {
        var command = new UpdateBrandLiveChatCommand()
        {
            BrandId = brandId,
            Name = request.Name,
            Setting = JsonDocument.Parse(request.Setting)
        };
        await updateService.ExecuteAsync(command).ConfigureAwait(false);

        return CreateValidResponse<MessageResponse>();
    }
}