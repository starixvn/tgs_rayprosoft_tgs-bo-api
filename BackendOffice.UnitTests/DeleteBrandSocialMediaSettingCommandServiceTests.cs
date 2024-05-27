using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

public class DeleteBrandSocialMediaSettingCommandServiceTests
{
    [Test]
    public async Task DeleteBrandSocialMediaSettingCommandService_刪除社群連結設定()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new DeleteBrandSocialMediaSettingCommandService(fakeBrandServiceClient);

        var command = new DeleteBrandSocialMediaSettingCommand(Guid.NewGuid(), Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = fakeBrandServiceClient.Received(1)
            .DeleteSocialMediaSettingAsync(
                Arg.Is(command.BrandId),
                Arg.Is(command.SettingId));
    }
}
