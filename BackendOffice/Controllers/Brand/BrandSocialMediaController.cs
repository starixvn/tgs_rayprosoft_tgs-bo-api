using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.BrandCertificate.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/brand/social")]
[ApiController]
public class BrandSocialMediaController : BaseContextController
{
    [HttpGet("{brandId:Guid}", Name = "ListBrandSocialMediaSettings")]
    public IAsyncEnumerable<SocialMediaSettingViewModel> ListSocialMediaSettingsAsync(
        [FromServices] IQueryService<ListBrandSocialMediaSettingsQuery, SocialMediaSetting> queryService,
        Guid brandId)
    {
        var query = new ListBrandSocialMediaSettingsQuery(brandId);

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Select(setting => new SocialMediaSettingViewModel
            {
                Id = setting.Id,
                Name = setting.Name,
                Type = setting.Type,
                Url = setting.Url
            });
    }

    [HttpPut("{brandId:Guid}", Name = "AddSocialMediaSetting")]
    public ValueTask<Guid> AddSocialMediaSettingAsync(
        [FromServices] IGetService<AddBrandSocialMediaSettingCommand, Guid> addService,
        Guid brandId,
        [FromBody] AddSocialMediaSettingViewModel viewModel)
    {
        var command = new AddBrandSocialMediaSettingCommand(
            brandId,
            new AddSocialMediaSetting(viewModel.Name, viewModel.Type, viewModel.Url),
            Username);

        return addService.GetAsync(command, HttpContext.RequestAborted);
    }

    [HttpPatch("order/{settingId:Guid}", Name = "SetSocialMediaSettingOrder")]
    public ValueTask SetSocialMediaSettingOrderAsync(
        [FromServices] ICommandService<SetBrandSocialMediaSettingOrderCommand> setOrderService,
        Guid settingId,
        [FromBody] int newOrder)
    {
        var command = new SetBrandSocialMediaSettingOrderCommand(settingId, newOrder, Username);

        return setOrderService.ExecuteAsync(command, HttpContext.RequestAborted);
    }
}
