using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditSuspSettingGetService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : IGetService<BrandGameCreditSuspSettingGetQuery, BrandGameCreditSuspSettingResult>
{
    public async ValueTask<BrandGameCreditSuspSettingResult> GetAsync(BrandGameCreditSuspSettingGetQuery query, CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: query.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{query.BrandId}");

        var setting = await client.GameCreditSuspSettingGetAsync(
            brandCode: brandInfo.Code,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new BrandGameCreditSuspSettingResult(
            ResellerId: setting.ResellerId,
            ResellerCode: setting.ResellerCode,
            LicenseeId: setting.LicenseeId,
            LicenseeCode: setting.LicenseeCode,
            BrandId: setting.BrandId,
            BrandCode: setting.BrandCode,
            LowRisk: setting.LowRisk,
            LowNotification: new BrandGameCreditSuspSettingMsg(
                 EmailSubject: setting.LowNotification.EmailSubject,
                 EmailContent: setting.LowNotification.EmailContent),
            ModerateRisk: setting.ModerateRisk,
            ModerateNotification: new BrandGameCreditSuspSettingMsg(
                 EmailSubject: setting.ModerateNotification.EmailSubject,
                 EmailContent: setting.ModerateNotification.EmailContent),
            HighRisk: setting.HighRisk,
            HighNotification: new BrandGameCreditSuspSettingMsg(
                 EmailSubject: setting.HighNotification.EmailSubject,
                 EmailContent: setting.HighNotification.EmailContent),
            ManualNotification: new BrandGameCreditSuspSettingMsg(
                 EmailSubject: setting.ManualNotification.EmailSubject,
                 EmailContent: setting.ManualNotification.EmailContent),
             CreatedOn: setting.CreatedOn,
             UpdatedOn: setting.UpdatedOn);
    }
}
