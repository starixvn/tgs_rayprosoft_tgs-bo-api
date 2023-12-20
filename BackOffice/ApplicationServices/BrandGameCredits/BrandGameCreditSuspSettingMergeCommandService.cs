using System;
using System.Threading;
using System.Threading.Tasks;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.ServiceUtility;

namespace TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

internal class BrandGameCreditSuspSettingMergeCommandService(
    IBrandServiceClient brandServiceClient,
    IBrandGameCreditsClient client) : ICommandService<BrandGameCreditSuspSettingMergeCommand>
{
    public async ValueTask ExecuteAsync(BrandGameCreditSuspSettingMergeCommand command, CancellationToken cancellationToken = default)
    {
        var brandInfo = await brandServiceClient.GetBrandInfoAsync(
            brandId: command.BrandId,
            cancellationToken: cancellationToken)
            ?? throw new NullReferenceException($"This brand does not exist.{command.BrandId}");

        await client.GameCreditSuspSettingMergeAsync(
            brandCode: brandInfo.Code,
            lowRisk: command.LowRisk,
            lowNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailSubject = command.LowNotification.EmailSubject,
                EmailContent = command.LowNotification.EmailContent
            },
            moderateRisk: command.ModerateRisk,
            moderateNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailSubject = command.ModerateNotification.EmailSubject,
                EmailContent = command.ModerateNotification.EmailContent
            },
            highRisk: command.HighRisk,
            highNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailSubject = command.HighNotification.EmailSubject,
                EmailContent = command.HighNotification.EmailContent
            },
            manualNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailSubject = command.ManualNotification.EmailSubject,
                EmailContent = command.ManualNotification.EmailContent
            },
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
