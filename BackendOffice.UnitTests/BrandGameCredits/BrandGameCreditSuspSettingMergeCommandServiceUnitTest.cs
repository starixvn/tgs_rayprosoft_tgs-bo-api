using NSubstitute;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditSuspSettingMergeCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var command = new BrandGameCreditSuspSettingMergeCommand(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            LowRisk: 60m,
            LowNotification: new BrandGameCreditSuspSettingMsg(
                EmailSubject: "[Licensee][Brand] 加值通知 Warning: Balance low",
                EmailContent: "敬愛的夥伴您好，\r\n您的平台目前剩餘額度不足，為避免影響會員遊戲體驗，請盡速聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a high balance usage. To maintain the stability and quality of your gaming experience, we kindly recommend reaching out to your designated business contact for a top-up. Thank you."
                ),
            ModerateRisk: 85m,
            ModerateNotification: new BrandGameCreditSuspSettingMsg(
                EmailSubject: "[Licensee][Brand] 緊急加值通知 Warning: Balance too low",
                EmailContent: "敬愛的夥伴您好，\r\n您的平台目前剩餘額度嚴重不足，所有遊戲暫時不可進入；為避免影響平台正常運營，請立即聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a higher balance usage, and the access to all games will be temporarily suspended. To maintain the stability of your site, we kindly recommend reaching out to your designated business contact for a top-up immediately. Thank you."
                ),
            HighRisk: 95m,
            HighNotification: new BrandGameCreditSuspSettingMsg(
                EmailSubject: "[Licensee][Brand] 平台已停用，請立即加值 Notice of Suspension",
                EmailContent: "貴司您好，\r\n由於您的平台剩餘額度已低於限制，平台及所有遊戲暫時不可進入；請立即聯繫貴司對應商務進行加值，並由專人協助您恢復平台運行，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site has been temporarily suspended due to a balance usage that has exceeded the limit. To recover the full access of platform and all games, we kindly recommend reaching out to your designated business contact for a top-up immediately. Thank you."
                ),
            ManualNotification: new BrandGameCreditSuspSettingMsg(
                EmailSubject: "[Licensee][Brand] 加值通知 Warning: Balance low",
                EmailContent: "敬愛的夥伴您好，\r\n您的平台目前剩餘額度不足，為避免影響會員遊戲體驗，請盡速聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a high balance usage. To maintain the stability and quality of your gaming experience, we kindly recommend reaching out to your designated business contact for a top-up. Thank you."
                ));

        var brandInfo = new BrandInfo
        {
            Id = Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            Code = "GPI",
            Status = BrandStatus.Active,
            Currency = "CNY",
            SmsEnabled = false,
            SmsRegisterFlowEnabled = false,
            PlayerIdentityEnabled = false,
            EmailRegisterFlowEnabled = false,
            BankInfoRegisterFlowEnabled = false,
            TimeSpanMinutes = 480
        };
        _ = fakeBrandServiceClient.GetBrandInfoAsync(
            brandId: Arg.Is(command.BrandId),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var service = new BrandGameCreditSuspSettingMergeCommandService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        await service.ExecuteAsync(command);

        _ = fakeBrandGameCreditsClient
            .Received(1)
            .GameCreditSuspSettingMergeAsync(
            brandCode: Arg.Is(brandInfo.Code),
            lowRisk: Arg.Is(command.LowRisk),
            lowNotification: Arg.Any<BrandGameCreditSuspSettingNotificationMsg>(),
            moderateRisk: Arg.Is(command.ModerateRisk),
            moderateNotification: Arg.Any<BrandGameCreditSuspSettingNotificationMsg>(),
            highRisk: Arg.Is(command.HighRisk),
            highNotification: Arg.Any<BrandGameCreditSuspSettingNotificationMsg>(),
            manualNotification: Arg.Any<BrandGameCreditSuspSettingNotificationMsg>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
