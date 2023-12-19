using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework.Legacy;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.BrandGameCredits.Common;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;
using TGS.Services.BackOffice.ApplicationServices.NotificationContacts;
using Tgs.Notification.Contacts.Client;
using Tgs.BrandCertificate;
using Tgs.Notification.Contacts.Common.Models;

namespace TGS.Tests.BackendOffice.UnitTests.NotificationContacts;

public class NotificationContactEmailBrandQueryServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeNotificationContactsClient = Substitute.For<INotificationContactsClient>();

        var query = new NotificationContactEmailBrandQuery(
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

        var emailContact = new EmailContactGet(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            Contacts: new EmailContact[] { new() {
                Name="test.tt",
                Email= "test.tt@rayprosoft.com",
                Remark= "test"} });
        _ = fakeNotificationContactsClient
            .EmailBrandGetAsync(
            code: Arg.Is(brandInfo.Code),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(emailContact);

        var service = new NotificationContactEmailBrandQueryService(
            fakeBrandServiceClient,
            fakeNotificationContactsClient);

        var actual = await service.QueryAsync(query)
            .ToArrayAsync();

        ClassicAssert.True(actual.Length != 0);
        ClassicAssert.AreEqual("test.tt", actual.First()!.Name);
        ClassicAssert.AreEqual("test.tt@rayprosoft.com", actual.First()!.Email);
        ClassicAssert.AreEqual("test", actual.First()!.Remark);
    }
}
