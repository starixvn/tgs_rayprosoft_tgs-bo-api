using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.SmsService.Client;

namespace TGS.Apis.BackendOffice.Controllers.Brand;

[Route("api/brand/sms")]
[ApiController]
public class BrandSmsController : BaseContextController
{
    [HttpPost("{brandId:Guid}", Name = "AddBrandSms")]
    public ValueTask AddBrandSmsAsync(
        [FromServices] ICommandService<AddBrandSmsCommand> commandService,
        Guid brandId,
        [FromBody] Guid smsSettingId)
    {
        var command = new AddBrandSmsCommand(
            brandId,
            smsSettingId);

        return commandService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpDelete("{brandId:Guid}/{smsSettingId:Guid}", Name = "RemoveBrandSms")]
    public ValueTask RemoveBrandSmsAsync(
        [FromServices] ICommandService<RemoveBrandSmsCommand> commandService,
        Guid brandId,
        Guid smsSettingId)
    {
        var command = new RemoveBrandSmsCommand(brandId, smsSettingId);

        return commandService.ExecuteAsync(command, HttpContext.RequestAborted);
    }

    [HttpGet("options/{brandId:Guid}", Name = "ListBrandSms")]
    public IAsyncEnumerable<BrandSmsSettingViewModel> ListBrandSmsAsync(
        [FromServices] IQueryService<ListBrandSmsQuery, BrandSmsInfo> queryService,
        Guid brandId)
    {
        var query = new ListBrandSmsQuery(brandId);

        return queryService.QueryAsync(query, HttpContext.RequestAborted)
            .Select(info => new BrandSmsSettingViewModel
            {
                SmsSettingId = info.SmsSettingId,
                Name = info.SmsSettingName,
            });
    }

    [HttpPatch("apply/{brandId:Guid}", Name = "ApplyBrandSms")]
    public ValueTask ApplyBrandSmsAsync(
       [FromServices] ICommandService<ApplyBrandSmsCommand> commandService,
       Guid brandId,
       [FromBody] Guid smsSettingId)
       => commandService.ExecuteAsync(new ApplyBrandSmsCommand(
           brandId,
           smsSettingId,
           Username),
           HttpContext.RequestAborted);

    [HttpGet("{brandId}", Name = "GetBrandSmsInfo")]
    public async ValueTask<BrandSmsResultViewModel> GetBrandSmsInfoAsync(
        [FromServices] IGetService<GetBrandSmsQuery, BrandSmsResult> getService,
        Guid brandId)
    {
        var query = new GetBrandSmsQuery(brandId);

        var info = await getService
            .GetAsync(query, HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new BrandSmsResultViewModel
        {
            EnableSms = info.EnableSms,
            SmsVendorCode = info.SmsVendorCode,
        };
    }
}