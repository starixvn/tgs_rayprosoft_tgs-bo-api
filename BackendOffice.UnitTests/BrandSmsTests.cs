using TGS.Services.BackOffice.ApplicationServices;
using TGS.SmsService.Client;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class BrandSmsTests
{
    [Test]
    public async Task AddBrandSmsCommandService_新增平台簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new AddBrandSmsCommandService(fakeClient);

        var brandId = Guid.NewGuid();
        var smsSettingId = Guid.NewGuid();
        var command = new AddBrandSmsCommand(brandId, smsSettingId);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeClient.Received(1)
            .AddBrandSmsAsync(
                Arg.Is(brandId),
                Arg.Is(smsSettingId));
    }

    [Test]
    public async Task RemoveBrandSmsCommandService_移除平台簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new RemoveBrandSmsCommandService(fakeClient);

        var brandId = Guid.NewGuid();
        var smsSettingId = Guid.NewGuid();

        var command = new RemoveBrandSmsCommand(brandId, smsSettingId);
        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeClient.Received(1)
            .RemoveBrandSmsAsyncAsync(
                Arg.Is(brandId),
                Arg.Is(smsSettingId));
    }

    [Test]
    public async Task GetBrandSmsQueryService_取得平台簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new ListBrandSmsQueryService(fakeClient);

        var brandId = Guid.NewGuid();
        var query = new ListBrandSmsQuery(brandId);

        // Act
        _ = await sut.QueryAsync(query).ToArrayAsync();

        // Assert
        _ = fakeClient.Received(1)
            .QueryBrandSmsListAsync(Arg.Is(brandId));
    }
}