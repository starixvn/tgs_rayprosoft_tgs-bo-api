using System.Text.Json;
using TGS.Services.BackOffice.ApplicationServices.Sms;
using TGS.SmsService.Client;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class SmsTests
{
    [Test]
    public async Task AddSmsSettingCommandService_新增簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new AddSmsSettingCommandService(fakeClient);

        var name = "name";
        var code = "code";
        var setting = JsonDocument.Parse("{}");
        var modifier = "test";

        // Act
        var command = new AddSmsSettingCommand(name, code, setting, modifier);
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeClient.Received(1)
            .AddSmsSettingAsync(
                Arg.Is(name),
                Arg.Is(code),
                Arg.Is(setting),
                Arg.Is(modifier));
    }

    [Test]
    public async Task RemoveSmsSettingCommandService_移除簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new RemoveSmsSettingCommandService(fakeClient);

        var id = Guid.NewGuid();
        var command = new RemoveSmsSettingCommand(id);

        // Act
        await sut.ExecuteAsync(command);

        // Assert
        _ = fakeClient.Received(1)
            .RemoveSmsSettingAsync(Arg.Is(id));
    }

    [Test]
    public async Task UpdateSmsSettingCommandService_更新簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new UpdateSmsSettingCommandService(fakeClient);

        var id = Guid.NewGuid();
        var name = "name";
        var code = "code";
        var setting = JsonDocument.Parse("{}");
        var modifier = "test";

        var command = new UpdateSmsSettingCommand(id, name, code, setting, modifier);
        // Act
        await sut.ExecuteAsync(command);
        // Assert
        _ = fakeClient.Received(1)
            .UpdateSmsSettingAsync(
            Arg.Is(id),
            Arg.Is(name),
            Arg.Is(code),
            Arg.Is(setting),
            Arg.Is(modifier));
    }

    [Test]
    public async Task GetSmsSettingsQueryService_查詢全部簡訊設定()
    {
        // Arrange
        var fakeClient = Substitute.For<ISmsServiceClient>();
        var sut = new GetSmsSettingsQueryService(fakeClient);

        // Act
        _ = await sut.QueryAsync(new GetSmsSettingsQuery()).ToArrayAsync();
        // Assert
        _ = fakeClient.Received(1)
            .QuerySmsSettingListAsync();
    }
}