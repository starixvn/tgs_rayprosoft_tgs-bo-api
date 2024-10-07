using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

public class SetBrandSocialMediaSettingOrderCommandServiceTests
{
    [Test]
    public async Task SetBrandSocialMediaSettingOrderCommandService_設定社群連結資料排序()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new SetBrandSocialMediaSettingOrderCommandService(fakeBrandServiceClient);

        var command = new SetBrandSocialMediaSettingOrderCommand(
            Guid.NewGuid(),
            200,
            "test1");

        await sut.ExecuteAsync(command);

        _ = fakeBrandServiceClient.Received(1)
            .SetSocialMediaSettingOrderAsync(
                Arg.Is(command.SettingId),
                Arg.Is(command.NewOrder),
                Arg.Is(command.Modifier));
    }
}
