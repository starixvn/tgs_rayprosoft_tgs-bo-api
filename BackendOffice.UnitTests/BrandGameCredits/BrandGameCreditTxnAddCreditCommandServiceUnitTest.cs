using System.Text.Json.Nodes;
using Tgs.BrandCertificate;
using Tgs.BrandCertificate.Clients;
using Tgs.BrandGameCredits.Client;
using TGS.Services.BackOffice.ApplicationServices.BrandGameCredits;

namespace TGS.Tests.BackendOffice.UnitTests.BrandGameCredits;

public class BrandGameCreditTxnAddCreditCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();

        var command = new BrandGameCreditTxnAddCreditCommand(
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            Money: 1m,
            Amount: 10m,
            Remark: "test",
            Expansions: new JsonObject
            {
                ["User"] = "test"
            },
            CreatedUserId: Guid.NewGuid(),
            CreatedUsername: string.Empty);

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

        var service = new BrandGameCreditTxnAddCreditCommandService(
            fakeBrandServiceClient,
            fakeBrandGameCreditsClient);

        await service.ExecuteAsync(command);

        _ = fakeBrandGameCreditsClient
            .Received(1)
            .GameCreditTxnAddCreditAsync(
            Arg.Is(brandInfo.Code),
            Arg.Is(command.Money),
            Arg.Is(command.Amount),
            Arg.Is(command.Remark),
            Arg.Is(command.Expansions),
            Arg.Is(command.CreatedUserId),
            Arg.Is(command.CreatedUsername),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
