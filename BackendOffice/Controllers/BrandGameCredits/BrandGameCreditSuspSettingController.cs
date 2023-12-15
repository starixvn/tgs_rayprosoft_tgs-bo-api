using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TGS.Apis.BackendOffice.ViewModels;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.BrandGameCredits;

[Route("api/[controller]")]
[ApiController]
public class BrandGameCreditSuspSettingController : BaseContextController
{
    [HttpGet("{brandId:guid}", Name = "BrandGameCreditSuspSetting_Get")]
    public async ValueTask<BrandGameCreditSuspSettingViewModel> GetAsync(
        [FromServices] IGetService<BrandGameCreditSuspSettingGetQuery, BrandGameCreditSuspSettingResult> service,
        Guid brandId)
    {
        var result = await service.GetAsync(
            query: new BrandGameCreditSuspSettingGetQuery(brandId),
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new BrandGameCreditSuspSettingViewModel(
            ResellerId: result.ResellerId,
            ResellerCode: result.ResellerCode,
            LicenseeId: result.LicenseeId,
            LicenseeCode: result.LicenseeCode,
            BrandId: result.BrandId,
            BrandCode: result.BrandCode,
            LowRisk: result.LowRisk,
            LowNotification: new BrandGameCreditSuspSettingMsgViewModel
            {
                EmailSubject = result.LowNotification.EmailSubject,
                EmailContent = result.LowNotification.EmailContent
            },
            ModerateRisk: result.ModerateRisk,
            ModerateNotification: new BrandGameCreditSuspSettingMsgViewModel
            {
                EmailSubject = result.ModerateNotification.EmailSubject,
                EmailContent = result.ModerateNotification.EmailContent
            },
            HighRisk: result.HighRisk,
            HighNotification: new BrandGameCreditSuspSettingMsgViewModel
            {
                EmailSubject = result.HighNotification.EmailSubject,
                EmailContent = result.HighNotification.EmailContent
            },
            ManualNotification: new BrandGameCreditSuspSettingMsgViewModel
            {
                EmailSubject = result.ManualNotification.EmailSubject,
                EmailContent = result.ManualNotification.EmailContent
            },
             CreatedOn: result.CreatedOn,
             UpdatedOn: result.UpdatedOn);
    }

    [HttpPost("{brandId:guid}", Name = "BrandGameCreditSuspSetting_Merge")]
    public ValueTask MergeAsync(
        [FromServices] ICommandService<BrandGameCreditSuspSettingMergeCommand> service,
        Guid brandId,
        [FromBody] BrandGameCreditSuspSettingMergeViewModel source)
        => service.ExecuteAsync(
            command: new BrandGameCreditSuspSettingMergeCommand(
                BrandId: brandId,
                LowRisk: source.LowRisk,
                LowNotification: new BrandGameCreditSuspSettingMsg(
                    source.LowNotification.EmailSubject,
                    source.LowNotification.EmailContent),
                ModerateRisk: source.ModerateRisk,
                ModerateNotification: new BrandGameCreditSuspSettingMsg(
                    source.ModerateNotification.EmailSubject,
                    source.ModerateNotification.EmailContent),
                HighRisk: source.HighRisk,
                HighNotification: new BrandGameCreditSuspSettingMsg(
                    source.HighNotification.EmailSubject,
                    source.HighNotification.EmailContent),
                ManualNotification: new BrandGameCreditSuspSettingMsg(
                    source.ManualNotification.EmailSubject,
                    source.ManualNotification.EmailContent)
                ),
            cancellationToken: HttpContext.RequestAborted);
}
