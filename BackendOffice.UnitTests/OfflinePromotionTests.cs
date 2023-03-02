using System.Reflection;
using Microsoft.Extensions.FileProviders;
using NSubstitute;
using NUnit.Framework;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Promotion.Offline;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class OfflinePromotionTests
{
    [Test]
    public async Task 下載Excel範本()
    {
        var sut = new ExportOfflinePromotionFormTemplateCommandService();

        var command = new ExportOfflinePromotionFormTemplateCommand();

        var actual = await sut.GetAsync(command);

        Assert.IsNotNull(actual);
    }

    [Test]
    public async Task 背景處理Temp_User_Batch資料()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();
        var fakePlayerInfosQueryService = Substitute.For<IQueryService<PlayerInfosByUserNamesQuery, PlayerInfo>>();

        var sut = new GetPlayerCodeByBatchCommandService(
            fakePlayerInfosQueryService,
            fakeBatchTempRepository);

        var brandId = Guid.NewGuid();
        var userNames = new[] { "username1", "username2" };
        var tempUserBatch = new OfflineTempUserBatch(brandId, userNames);

        var command = new GetPlayerCodeByBatchCommand(
            Guid.NewGuid());

        _ = fakeBatchTempRepository.BidBatchAsync(Arg.Is(command.BatchId))
            .Returns(true);

        _ = fakeBatchTempRepository
            .GetOfflineTempUserBatchAsync(
                Arg.Is(command.BatchId),
                Arg.Any<CancellationToken>())
            .Returns(tempUserBatch);

        await sut.ExecuteAsync(command);

        _ = fakePlayerInfosQueryService
            .Received()
            .QueryAsync(
                Arg.Any<PlayerInfosByUserNamesQuery>(),
                Arg.Any<CancellationToken>());

        _ = fakeBatchTempRepository
            .Received(1)
            .SaveProcessedOfflinePlayerItemsAsync(
                Arg.Is(command.BatchId),
                Arg.Any<OfflineUsersResult>(),
                Arg.Any<CancellationToken>());

        _ = fakeBatchTempRepository
            .Received(1)
            .CompleteBatchAsync(
                Arg.Is(command.BatchId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 儲存上傳的Temp_User_Batch資料()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new SaveOfflineTempUserBatchCommandService(fakeBatchTempRepository);

        var batchData = new OfflineTempUserBatch(
            Guid.NewGuid(),
            new[] { "user_name_1", "user_name_2" });

        var command = new SaveOfflineTempUserBatchCommand(
            batchData.BrandId,
            batchData.UserNames);

        _ = await sut.GetAsync(command);

        _ = fakeBatchTempRepository
            .Received(1)
            .SaveOfflineTempUserBatchAsync(
                Arg.Is(batchData),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 讀取Excel_UserList資料()
    {
        var sut = new ReadOfflinePromotionUserListQueryService();

        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var fileInfo = fileProvider.GetFileInfo("/Assets/TestOfflineLoginReward.xlsx");

        await using var fileStream = fileInfo.CreateReadStream();

        var query = new ReadOfflinePromotionUserListQuery(fileStream);

        var actual = await sut.QueryAsync(query).ToArrayAsync();

        Assert.AreEqual(5, actual.Length);
    }

    [Test]
    public async Task 取得處理後的UserList資料()
    {
        var fakeBatchTempRepository = Substitute.For<IBatchTempRepository>();

        var sut = new GetProcessedUsersQueryService(fakeBatchTempRepository);

        var query = new GetProcessedUsersQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakeBatchTempRepository
            .Received(1)
            .GetOfflineUsersResultAsync(
                Arg.Is(query.BatchId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 建立線下儲值活動()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new CreateOfflineDepositPromotionCommandService(fakePromotionServiceClient);

        var command = new CreateOfflineDepositPromotionCommand(
            Guid.NewGuid(),
            "OfflineDeposit",
            (DateTime.UtcNow.AddDays(1D), DateTime.UtcNow.AddDays(3D)),
            new OfflineDepositSettings(
                new[] { "test1", "test2" },
                3M,
                new OfflineDepositHierarchy(new[] {
                    new OfflineDepositPhase(100M, 20M),
                    new OfflineDepositPhase(500M, 50M) })),
            "Modifier");

        var expectedCreateInfo = new CreatePromotionMainInfo
        {
            Name = command.PromotionName,
            EventDuring = command.EventDuring,
            PromotionType = "OfflineDepositReward",
            IsAutoSent = true,
            PromotionLimitAmount = null
        };
        var promotionId = Guid.NewGuid();

        _ = fakePromotionServiceClient.CreatePromotionAsync(
            Arg.Is(command.BrandId),
            Arg.Is(expectedCreateInfo),
            Arg.Is(command.Modifier))
            .Returns(promotionId);

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Is(command.DepositSettings),
                Arg.Is(command.Modifier));
        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(PromotionStatus.Enabled),
                Arg.Is(command.Modifier));
    }

    [Test]
    public async Task 建立線下登入活動()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new CreateOfflineLoginPromotionCommandService(fakePromotionServiceClient);

        var command = new CreateOfflineLoginPromotionCommand(
            Guid.NewGuid(),
            "OfflineLogin",
            (DateTime.UtcNow.AddDays(1D), DateTime.UtcNow.AddDays(3D)),
            new OfflineLoginSettings(
                10M,
                2M,
                new[] { "test1", "test2" }),
            "Modifier");

        var expectedCreateInfo = new CreatePromotionMainInfo
        {
            Name = command.PromotionName,
            EventDuring = command.EventDuring,
            PromotionType = "OfflineLoginReward",
            IsAutoSent = true,
            PromotionLimitAmount = null
        };
        var promotionId = Guid.NewGuid();

        _ = fakePromotionServiceClient.CreatePromotionAsync(
            Arg.Is(command.BrandId),
            Arg.Is(expectedCreateInfo),
            Arg.Is(command.Modifier))
            .Returns(promotionId);

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Is(command.LoginSettings),
                Arg.Is(command.Modifier));
        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(promotionId),
                Arg.Is(PromotionStatus.Enabled),
                Arg.Is(command.Modifier));
    }
}
