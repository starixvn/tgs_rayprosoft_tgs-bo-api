using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.Controllers.Messages.Settings;
using TGS.Apis.BackendOffice.Resources;
using TGS.Core.DataServices.Base.Interfaces;
using TGS.Core.Infrastructures.Redis;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Domain;
using Setting = TGS.Core.DataServices.Tgs.Models.Setting;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SettingController(
    ITgsQueries<Setting> queries,
    ITgsCommands<Setting> commands,
    IRedisCacheManager cacheManager) : BaseContextController
{
    [HttpPost("")]
    [Description("Create a setting")]
    public async Task<MessageResponse> CreateAsync([FromBody] CreateSettingRequest request)
    {
        var settingExisted = await queries.GetSingleAsync(p => p.Key == request.Key);
        if (settingExisted != null)
        {
            return CreateInvalidResponse<MessageResponse>(Resources.Setting.KeyExists);
        }

        var setting = new Setting
        {
            Key = request.Key,
            Value = request.Value,
            Description = request.Description,
            CreatedOn = DateTimeOffset.Now,
            CreatedByUsername = Username,
            UpdatedOn = DateTimeOffset.Now,
            UpdatedByUsername = Username
        };

        await commands.CreateAsync(setting);
        return CreateValidResponse<MessageResponse>();
    }

    [Route("{key}")]
    [HttpDelete]
    [Description("Delete setting by key")]
    public async Task<MessageResponse> DeleteAsync(string key)
    {
        var setting = await queries.GetSingleAsync(p => p.Key == key);
        if (setting == null)
        {
            return CreateInvalidResponse<MessageResponse>(Common.NotFound);
        }

        await commands.DeleteAsync(setting);
        return CreateValidResponse<MessageResponse>();
    }

    [Route("")]
    [HttpGet]
    [Description("Get setting list")]
    public Task<PagedResponse<SettingModel>> GetListAsync([FromQuery] GetSettingsRequest request)
        => queries.GetQueries()
            .Where(request.Key, p => p.Key.Contains(request.Key))
            .Select(setting => new SettingModel
            {
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                CreatedByUsername = setting.CreatedByUsername,
                UpdatedByUsername = setting.UpdatedByUsername,
                CreatedOn = setting.CreatedOn,
                UpdatedOn = setting.UpdatedOn
            })
            .CreatePagedResponseAsync(request);

    [Route("{key}")]
    [HttpGet]
    [Description("Get setting by key")]
    public async Task<GetSettingResponse> GetSingleAsync(string key)
    {
        var setting = await queries.GetSingleAsync(p => p.Key == key);

        return setting == null
            ? CreateInvalidResponse<GetSettingResponse>(Common.NotFound)
            : new()
            {
                Valid = true,
                Setting = new()
                {
                    Key = setting.Key,
                    Value = setting.Value,
                    Description = setting.Description,
                    CreatedByUsername = setting.CreatedByUsername,
                    UpdatedByUsername = setting.UpdatedByUsername,
                    CreatedOn = setting.CreatedOn,
                    UpdatedOn = setting.UpdatedOn
                }
            };
    }

    [HttpPut("{key}")]
    [Description("Update setting by key")]
    public async Task<MessageResponse> UpdateAsync(string key, [FromBody] UpdateSettingRequest request)
    {
        var setting = await queries.GetSingleAsync(p => p.Key == key);
        if (setting == null)
        {
            return CreateInvalidResponse<MessageResponse>(Common.NotFound);
        }

        setting.Value = request.Value;
        setting.Description = request.Description;
        setting.UpdatedOn = DateTimeOffset.Now;
        setting.UpdatedByUsername = Username;

        await commands.UpdateAsync(setting);

        await cacheManager.ClearCacheAsync(RedisCacheType.Setting, setting.Key);

        return CreateValidResponse<MessageResponse>();
    }
}
