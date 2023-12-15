using System.Text.Json;
using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditSuspSettingGetServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var query = new BrandGameCreditSuspSettingGetQuery(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"));

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
            brandId: Arg.Is(query.BrandId),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var setting = new BrandGameCreditSuspSetting(
           ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
           ResellerCode: "TGS",
           LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
           LicenseeCode: "TGS",
           BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
           BrandCode: "GPI",
           LowRisk: 60m,
           LowNotification: new BrandGameCreditSuspSettingNotificationMsg
           {
               EmailContent = "敬愛的夥伴您好，\r\n您的平台目前剩餘額度不足，為避免影響會員遊戲體驗，請盡速聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a high balance usage. To maintain the stability and quality of your gaming experience, we kindly recommend reaching out to your designated business contact for a top-up. Thank you.",
               EmailSubject = "[Licensee][Brand] 加值通知 Warning: Balance low"
           },
           ModerateRisk: 85m,
           ModerateNotification: new BrandGameCreditSuspSettingNotificationMsg
           {
               EmailContent = "敬愛的夥伴您好，\r\n您的平台目前剩餘額度嚴重不足，所有遊戲暫時不可進入；為避免影響平台正常運營，請立即聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a higher balance usage, and the access to all games will be temporarily suspended. To maintain the stability of your site, we kindly recommend reaching out to your designated business contact for a top-up immediately. Thank you.",
               EmailSubject = "[Licensee][Brand] 緊急加值通知 Warning: Balance too low"
           },
           HighRisk: 95m,
           HighNotification: new BrandGameCreditSuspSettingNotificationMsg
           {
               EmailContent = "貴司您好，\r\n由於您的平台剩餘額度已低於限制，平台及所有遊戲暫時不可進入；請立即聯繫貴司對應商務進行加值，並由專人協助您恢復平台運行，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site has been temporarily suspended due to a balance usage that has exceeded the limit. To recover the full access of platform and all games, we kindly recommend reaching out to your designated business contact for a top-up immediately. Thank you.",
               EmailSubject = "[Licensee][Brand] 平台已停用，請立即加值 Notice of Suspension"
           },
           ManualNotification: new BrandGameCreditSuspSettingNotificationMsg
           {
               EmailContent = "敬愛的夥伴您好，\r\n您的平台目前剩餘額度不足，為避免影響會員遊戲體驗，請盡速聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a high balance usage. To maintain the stability and quality of your gaming experience, we kindly recommend reaching out to your designated business contact for a top-up. Thank you.",
               EmailSubject = "[Licensee][Brand] 加值通知 Warning: Balance low"
           },
           CreatedOn: DateTimeOffset.Now,
           UpdatedOn: DateTimeOffset.Now);
        _ = fakeBrandGameCreditsClient
            .GameCreditSuspSettingGetAsync(
            brandCode: Arg.Is(brandInfo.Code),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(setting);

        var service = new BrandGameCreditSuspSettingGetService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        var actual = await service.GetAsync(query);

        ClassicAssert.NotNull(actual);
        ClassicAssert.AreEqual(Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"), actual.ResellerId);
        ClassicAssert.AreEqual("TGS", actual.ResellerCode);
        ClassicAssert.AreEqual(Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"), actual.LicenseeId);
        ClassicAssert.AreEqual("TGS", actual.LicenseeCode);
        ClassicAssert.AreEqual(Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"), actual.BrandId);
        ClassicAssert.AreEqual("GPI", actual.BrandCode);
        ClassicAssert.AreEqual(60m, actual.LowRisk);
        ClassicAssert.AreEqual(JsonSerializer.Serialize(setting.LowNotification), JsonSerializer.Serialize(actual.LowNotification));
        ClassicAssert.AreEqual(85m, actual.ModerateRisk);
        ClassicAssert.AreEqual(JsonSerializer.Serialize(setting.ModerateNotification), JsonSerializer.Serialize(actual.ModerateNotification));
        ClassicAssert.AreEqual(95m, actual.HighRisk);
        ClassicAssert.AreEqual(JsonSerializer.Serialize(setting.HighNotification), JsonSerializer.Serialize(actual.HighNotification));
        ClassicAssert.AreEqual(JsonSerializer.Serialize(setting.ManualNotification), JsonSerializer.Serialize(actual.ManualNotification));
    }
}
