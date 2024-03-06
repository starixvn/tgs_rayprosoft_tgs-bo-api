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
using System.Threading;

namespace TGS.Tests.BackendOffice.UnitTests.NotificationContacts;

public class NotificationContactEmailBrandCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeNotificationContactsClient = Substitute.For<INotificationContactsClient>();

        var command = new NotificationContactEmailBrandCommand(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            Emails: [ new(
                Name:"test.tt",
                Email:"test.tt@rayprosoft.com",
                Remark:"test") ]);

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

        var service = new NotificationContactEmailBrandCommandService(
            fakeBrandServiceClient,
            fakeNotificationContactsClient);

        await service.ExecuteAsync(command);

        _ = fakeNotificationContactsClient
            .Received(1)
            .EmailBrandMergeAsync(
            code: Arg.Is(brandInfo.Code),
            contacts: Arg.Any<IEnumerable<EmailContact>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
