using Tgs.BrandCertificate.Clients;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class UpsertBrandPlayerContractDisplayCommandServiceTests
{
    [Test]
    public async Task UpsertBrandPlayerContractDisplayCommandService_沒更新項目時不更新()
    {
        var fakeClient = Substitute.For<IBrandServiceClient>();

        var sut = new UpsertBrandPlayerContractDisplayCommandService(fakeClient);

        var command = new UpsertBrandPlayerContractDisplayCommand(
            Guid.NewGuid(),
            new Dictionary<string, bool>(),
            "system");

        await sut.ExecuteAsync(command);

        _ = fakeClient.Received(0)
            .SetBrandPlayerContractDisplayAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>());
    }

    [Test]
    public async Task UpsertBrandPlayerContractDisplayCommandService_出現非預期資料時不更新()
    {
        var fakeClient = Substitute.For<IBrandServiceClient>();

        var sut = new UpsertBrandPlayerContractDisplayCommandService(fakeClient);

        var command = new UpsertBrandPlayerContractDisplayCommand(
            Guid.NewGuid(),
            new Dictionary<string, bool>
            {
                { "abc", true }
            },
            "system");

        await sut.ExecuteAsync(command);

        _ = fakeClient.Received(0)
            .SetBrandPlayerContractDisplayAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>());
    }

    [Test]
    public async Task UpsertBrandPlayerContractDisplayCommandService_出現非玩家聯繫資訊時不更新()
    {
        var fakeClient = Substitute.For<IBrandServiceClient>();

        var sut = new UpsertBrandPlayerContractDisplayCommandService(fakeClient);

        var command = new UpsertBrandPlayerContractDisplayCommand(
            Guid.NewGuid(),
            new Dictionary<string, bool>
            {
                { PlayerPropertyName.RealName.ToString(), true }
            },
            "system");

        await sut.ExecuteAsync(command);

        _ = fakeClient.Received(0)
            .SetBrandPlayerContractDisplayAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>());
    }

    [Test]
    public async Task UpsertBrandPlayerContractDisplayCommandService_正常資料時更新()
    {
        var fakeClient = Substitute.For<IBrandServiceClient>();

        var sut = new UpsertBrandPlayerContractDisplayCommandService(fakeClient);

        var command = new UpsertBrandPlayerContractDisplayCommand(
            Guid.NewGuid(),
            new Dictionary<string, bool>
            {
                { PlayerPropertyName.Qq.ToString(), true }
            },
            "system");

        await sut.ExecuteAsync(command);

        _ = fakeClient.Received(1)
            .SetBrandPlayerContractDisplayAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>());
    }
}