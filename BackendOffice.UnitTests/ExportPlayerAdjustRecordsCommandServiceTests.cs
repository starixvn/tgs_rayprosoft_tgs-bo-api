using NSubstitute;
using Tgs.Agent.Client;
using Tgs.BrandCertificate.Clients;
using Tgs.CashQueryService.Client;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Adjust;

namespace TGS.Tests.BackendOffice.UnitTests;

public class ExportPlayerAdjustRecordsCommandServiceTests
{
    [Test]
    public async Task 匯出玩家調帳記錄()
    {
        var fakeCashQueryClient = Substitute.For<ICashQueryClient>();
        var fakeBrandRelationQueryService = Substitute.For<IQueryService<BrandRelationQuery, BrandRelationInfo>>();
        var fakeAgentClient = Substitute.For<IAgentClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new ExportAdjustRecordsCommandService(
            fakeCashQueryClient,
            fakeBrandRelationQueryService,
            fakeAgentClient,
            fakePlayerServiceClient);

        using var fs = new StubWriteStream();

        var command = new ExportPlayerAdjustRecordsCommand(
            Guid.NewGuid(),
            (DateTime.Now.AddDays(-1D), DateTime.Now),
            fs);

        _ = fakeCashQueryClient.CountPlayerAdjustRecordsAsync(Arg.Is(command.BrandId), Arg.Any<AdjustRecordQueryCondition>())
            .Returns(1);
        _ = fakeCashQueryClient.QueryPlayerAdjustRecordsAsync(
            Arg.Is(command.BrandId),
            Arg.Any<AdjustRecordQueryCondition>(),
            Arg.Any<int>())
            .Returns(new[]
            {
                new AdjustRecordListItem(
                    Guid.NewGuid(),
                    command.BrandId,
                    "test1",
                    Tgs.CashQueryService.AdjustType.CompanyAdjust,
                    100M,
                    500M,
                    "Manager",
                    "123",
                    DateTime.Now,
                    Tgs.CashQueryService.UserType.Player)
            }.ToAsyncEnumerable());

        _ = fakeBrandRelationQueryService.QueryAsync(Arg.Any<BrandRelationQuery>())
            .Returns(new[]
            {
                new BrandRelationInfo(
                    command.BrandId,
                    "AAA",
                    "AAA",
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty,
                    Guid.NewGuid(),
                    string.Empty,
                    string.Empty)
            }.ToAsyncEnumerable());

        _ = fakePlayerServiceClient.QueryPlayerInfoListAsync(Arg.Is(command.BrandId), Arg.Any<IEnumerable<string>>())
            .Returns(new[]
            {
                new PlayerInfo
                {
                    PlayerCode = "test1",
                    UserName = "Player1"
                }
            }.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);

        Assert.Greater(fs.Length, 0L);
    }
}
