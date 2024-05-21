using Tgs.BrandCertificate.Clients;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

public class AddBrandSocialMediaSettingCommandServiceTests
{
    [Test]
    public async Task AddBrandSocialMediaSettingCommandService_新增Brand社群連結()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new AddBrandSocialMediaSettingCommandService(fakeBrandServiceClient);

        var command = new AddBrandSocialMediaSettingCommand(
            Guid.NewGuid(),
            new AddSocialMediaSetting(
                "Test",
                Tgs.BrandCertificate.SocialType.Facebook,
                new Uri("https://localhost")),
            "System");

        _ = await sut.GetAsync(command);

        _ = fakeBrandServiceClient.Received(1)
            .AddSocialMediaSettingAsync(
                Arg.Is(command.BrandId),
                Arg.Is(command.Setting),
                Arg.Is(command.Modifier));
    }
}
