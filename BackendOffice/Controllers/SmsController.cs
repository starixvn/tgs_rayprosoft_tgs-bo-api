#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.Sms;
using TGS.SmsService.Client;

namespace TGS.Apis.BackendOffice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SmsController : BaseContextController
{
    [HttpPost(Name = "AddSmsSetting")]
    public ValueTask AddSmsSettingAsync(
       [FromServices] ICommandService<AddSmsSettingCommand> commandService,
       [FromBody] AddSmsSettingRequest request)
    {
        var command = new AddSmsSettingCommand(
            request.Name,
            request.Code,
            JsonDocument.Parse(string.IsNullOrWhiteSpace(request.Setting)
                ? "{}" 
                : request.Setting),
            Username);

        return commandService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpDelete("{smsSettingId:Guid}", Name = "RemoveSmsSetting")]
    public ValueTask RemoveSmsSettingAsync(
        [FromServices] ICommandService<RemoveSmsSettingCommand> commandService,
        Guid smsSettingId)
    {
        var command = new RemoveSmsSettingCommand(smsSettingId);

        return commandService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpPut("{smsSettingId:Guid}", Name = "UpdateSmsSetting")]
    public ValueTask UpdateSmsSettingAsync(
        [FromServices] ICommandService<UpdateSmsSettingCommand> commandService,
        [FromBody] UpdateSmsSettingRequest request,
        Guid smsSettingId)
    {
        var command = new UpdateSmsSettingCommand(
            smsSettingId,
            request.Name,
            request.Code,
            JsonDocument.Parse(request.Setting),
            Username);

        return commandService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpGet(Name = "QuerySmsSettingList")]
    public IAsyncEnumerable<SmsSettingViewModel> QuerySmsSettingListAsync(
        [FromServices] IQueryService<GetSmsSettingsQuery, SmsSettingInfo> queryService)
    {
        var info = queryService
            .QueryAsync(new GetSmsSettingsQuery(), HttpContext.RequestAborted);

        return info.Select(x => new SmsSettingViewModel
        {
            SmsSettingId = x.SmsSettingId,
            Name = x.Name,
            Code = x.Code,
            Setting = x.Setting,
        });
    }
}