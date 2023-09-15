using ExpectedObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.CashService.Client;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.Service.PlayerBuckets;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.ApplicationServices.Adjust;
using TGS.Services.BackOffice.ApplicationServices.PlayerBuckets;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PlayerBucketsTests
{
    [Test]
    public async Task Can_Get_Player_Bucket_Histories()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetPlayerBucketHistoriesQueryService(fakePlayerBucketClient);

        var query = new GetPlayerBucketHistoriesQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClient.Received(1)
            .GetBucketEventsAsync(Arg.Is(query.BucketId));
    }

    [Test]
    public async Task Can_Get_Player_Buckets()
    {
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new GetPlayerBucketsQueryService(
            fakePlayerBucketForTypesClient,
            fakePlayerServiceClient);

        var query = new GetPlayerBucketsQuery("test1", "Game1");

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(query.PlayerCode))
            .Returns(new PlayerInfo());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketForTypesClient.Received(1)
            .GetBucketsByPlayerCodeAsync(
                Arg.Is(query.PlayerCode),
                Arg.Is(query.BucketType));
    }

    [Test]
    public async Task Get_Player_Bucket_Amount()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetPlayerBucketAmountQueryService(fakePlayerBucketClient);

        var query = new GetPlayerBucketAmountQuery("test1");

        _ = await sut.GetAsync(query);

        _ = fakePlayerBucketClient.Received(1)
            .ReadBucketsAsync(Arg.Is(query.PlayerCode));
    }

    [Test]
    public async Task 查詢清除流水表單()
    {
        var fakePlayerBucketClent = Substitute.For<IPlayerBucketClient>();

        var sut = new GetCleanBucketFormListQueryService(fakePlayerBucketClent);

        var query = new GetCleanBucketFormListQuery("test1", CleanFlowStatus.Completed);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClent.Received(1)
            .QueryCleanBucketFomsAsync(
                Arg.Is(query.PlayerCode),
                Arg.Is(query.FlowStatus));
    }

    [Test]
    public async Task 清除流水依據PlayerCode及AnchorBucketId()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new ClearPlayerBucketsCommandService(
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient);

        var command = new ClearPlayerBucketsCommand("test1", Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.AnchorId),
                Arg.Is(default(string)),
                Arg.Any<string>());
    }

    [Test]
    public async Task 清除流水依據傳入的PlayerCode及TypeBucketResult集合()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();

        var sut = new ClearPlayerBucketsCommandService(
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient);

        var command = new ClearPlayerTypeBucketsCommand(
            "test1",
            new[] { "Game1", "Game2" });

        var typeBucketResults = new[]
        {
            new TypeBucketResult(
                null,
                new ReadBucketResult(1000M, Guid.NewGuid())),
            new TypeBucketResult(
                "Game1",
                new ReadBucketResult(2000M, Guid.NewGuid())),
            new TypeBucketResult(
                "Game2",
                new ReadBucketResult(3000M, Guid.NewGuid()))
        };

        _ = fakePlayerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            Arg.Is(command.PlayerCode),
            Arg.Is(command.BucketTypes))
            .Returns(typeBucketResults.ToAsyncEnumerable());

        var expectedAnchorBucketIds = typeBucketResults
            .Select(result => (result.BucketType, BucketAnchorId: result.Result.AnchorBucketId!.Value))
            .ToArray();

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketForTypesClient.Received(1)
            .CleanBucketsForBucketTypesAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is<IEnumerable<(string? BucketType, Guid BucketAnchorId)>>(
                    c => c.SequenceEqual(expectedAnchorBucketIds)),
                Arg.Any<string>(),
                Arg.Any<string>());
    }

    [Test]
    public async Task 處理清流水表單_如果餘額大於清除允許的額度_拒絕處理()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeGameProviderQueryService = Substitute.For<IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse>>();

        var sut = new ProcessCleanBucketsRequestCommandService(
            fakePlayerBucketClient,
            fakeCashServiceClient,
            fakeGameProviderQueryService,
            NullLogger<ProcessCleanBucketsRequestCommandService>.Instance);

        var command = new ProcessCleanBucketsRequestCommand(Guid.NewGuid());

        var bucketAnchorId = Guid.NewGuid();
        var form = new CleanBucketForm(
            command.FormId,
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            0M,
            new[]
            {
                new TypeBucketResult(
                    null,
                    new ReadBucketResult(1000M, bucketAnchorId))
            },
            Guid.NewGuid(),
            5M,
            null,
            null,
            CleanFlowStatus.Create,
            DateTime.UtcNow);

        _ = fakePlayerBucketClient.GetCleanBucketFormAsync(Arg.Is(command.FormId))
            .Returns(form);

        var cleaningBucketResult = new ReadBucketResult(
            1000M,
            Guid.NewGuid());
        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is("test1"))
            .Returns(cleaningBucketResult);

        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(form.WalletId))
            .Returns(0M);

        _ = fakeGameProviderQueryService.QueryAsync(Arg.Any<GetPlayerGameProviderBalanceQuery>())
            .Returns(new[]
            {
                new GetPlayerGameProviderBalanceResponse(
                    "Test",
                    "Test",
                    false,
                    false,
                    1000M)
            }.ToAsyncEnumerable());

        var expected = new
        {
            CleaningBalance = 0M,
            CleaningBucketAmounts = new[]
            {
                new TypeBucketResult(
                    null,
                    cleaningBucketResult)
            }
        };

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketClient.Received(0)
                .CleanBucketsAsync(
                    Arg.Is(form.PlayerCode),
                    Arg.Is(bucketAnchorId),
                    Arg.Is(form.DepositFormId.ToString()),
                    Arg.Is(default(string)));

        _ = fakePlayerBucketClient.Received(1)
            .RejectCleanBucketsAsync(
                Arg.Is(form.Id),
                Arg.Is<ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>>(
                    req => expected.ToExpectedObject().Matches(req)));
    }

    [Test]
    public async Task 處理清除流水表單()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeGameProviderQueryService = Substitute.For<IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse>>();

        var sut = new ProcessCleanBucketsRequestCommandService(
            fakePlayerBucketClient,
            fakeCashServiceClient,
            fakeGameProviderQueryService,
            NullLogger<ProcessCleanBucketsRequestCommandService>.Instance);

        var command = new ProcessCleanBucketsRequestCommand(Guid.NewGuid());

        var bucketAnchorId = Guid.NewGuid();

        var form = new CleanBucketForm(
                command.FormId,
                "test1",
                Guid.NewGuid(),
                Guid.NewGuid(),
                0M,
                new[]
                {
                    new TypeBucketResult(
                        null,
                        new ReadBucketResult(1000M, bucketAnchorId))
                },
                Guid.NewGuid(),
                5M,
                null,
                null,
                CleanFlowStatus.Create,
                DateTime.UtcNow);

        _ = fakePlayerBucketClient.GetCleanBucketFormAsync(Arg.Is(command.FormId))
            .Returns(form);

        var cleaningBucketResult = new ReadBucketResult(
            1000M,
            Guid.NewGuid());
        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is("test1"))
            .Returns(cleaningBucketResult);

        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(form.WalletId))
            .Returns(0M);

        _ = fakeGameProviderQueryService.QueryAsync(Arg.Any<GetPlayerGameProviderBalanceQuery>())
            .Returns(new[]
            {
                new GetPlayerGameProviderBalanceResponse(
                    "Test",
                    "Test",
                    false,
                    false,
                    0M)
            }.ToAsyncEnumerable());

        var expected = new
        {
            CleaningBalance = 0M,
            CleaningBucketAmounts = new[]
            {
                new TypeBucketResult(
                    null,
                    cleaningBucketResult)
            }
        };

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketClient.Received(1)
            .CleanBucketsAsync(
                Arg.Is(form.PlayerCode),
                Arg.Is(bucketAnchorId),
                Arg.Is(form.DepositFormId.ToString()),
                Arg.Is(default(string)));

        _ = fakePlayerBucketClient.Received(1)
            .CompleteCleanBucketsAsync(
                Arg.Is(form.Id),
                Arg.Is<ReviewCleanBucketRequest<GetPlayerGameProviderBalanceResponse[]>>(
                    req => expected.ToExpectedObject().Matches(req)));
    }

    [Test]
    public async Task 處理清除流水表單_如果遊戲廠商在維護中不處理()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();
        var fakeGameProviderQueryService = Substitute.For<IQueryService<GetPlayerGameProviderBalanceQuery, GetPlayerGameProviderBalanceResponse>>();

        var sut = new ProcessCleanBucketsRequestCommandService(
            fakePlayerBucketClient,
            fakeCashServiceClient,
            fakeGameProviderQueryService,
            NullLogger<ProcessCleanBucketsRequestCommandService>.Instance);

        var command = new ProcessCleanBucketsRequestCommand(Guid.NewGuid());

        var form = new CleanBucketForm(
                command.FormId,
                "test1",
                Guid.NewGuid(),
                Guid.NewGuid(),
                0M,
                new[]
                {
                    new  TypeBucketResult(
                        null,
                        new ReadBucketResult(1000M, Guid.NewGuid()))
                },
                Guid.NewGuid(),
                5M,
                null,
                null,
                CleanFlowStatus.Create,
                DateTime.UtcNow);

        _ = fakePlayerBucketClient.GetCleanBucketFormAsync(Arg.Is(command.FormId))
            .Returns(form);

        _ = fakePlayerBucketClient.ReadBucketsAsync(Arg.Is("test1"))
            .Returns(new ReadBucketResult(
                1000M,
                Guid.NewGuid()));

        _ = fakeCashServiceClient.GetBalanceAsync(Arg.Is(form.WalletId))
            .Returns(0M);

        _ = fakeGameProviderQueryService.QueryAsync(Arg.Any<GetPlayerGameProviderBalanceQuery>())
            .Returns(new[]
            {
                new GetPlayerGameProviderBalanceResponse(
                    "Test",
                    "Test",
                    false,
                    true,
                    0M)
            }.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketClient.Received(0)
            .CleanBucketsAsync(
                Arg.Is(form.PlayerCode),
                Arg.Is(form.CurrentBucketAmounts.First().Result.AnchorBucketId!.Value),
                Arg.Is(form.DepositFormId.ToString()),
                Arg.Is(default(string)));
    }

    [Test]
    public async Task 調整玩家流水_減少流水()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketCleint = Substitute.For<IPlayerBucketClient>();

        var sut = new AdjustPlayerBucketCommandService(
            fakePlayerServiceClient,
            fakePlayerBucketCleint);

        var command = new AdjustPlayerBucketCommand("test1", -1000M, "remark");

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new PlayerInfo());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketCleint.Received(1)
            .FillInBucketAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(-command.AdjustAmount),
                Arg.Is(FillType.Adjust),
                Arg.Is(default(string)),
                Arg.Is(command.Remark));
    }

    [Test]
    public async Task 調整玩家流水_增加流水()
    {
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketCleint = Substitute.For<IPlayerBucketClient>();

        var sut = new AdjustPlayerBucketCommandService(
            fakePlayerServiceClient,
            fakePlayerBucketCleint);

        var command = new AdjustPlayerBucketCommand("test1", 1000M, "remark");

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new PlayerInfo());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketCleint.Received(1)
            .CreateBucketAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.AdjustAmount),
                Arg.Is(BucketType.Adjustment),
                Arg.Is(default(string)),
                Arg.Is(command.Remark));
    }

    [Test]
    public async Task Can_Get_Brand_Bucket_Types()
    {
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();

        var sut = new GetBrandBucketTypesQueryService(
            fakePlayerBucketClient);

        var brandId = Guid.NewGuid();

        var query = new GetBrandBucketTypesQuery(brandId);

        var bucketTypes = new[] { "SportsBook", "Lottery" };

        _ = fakePlayerBucketClient.GetBrandBucketTypesAsync(Arg.Is(brandId))
            .Returns(bucketTypes.ToAsyncEnumerable());

        var response = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePlayerBucketClient.Received(1)
            .GetBrandBucketTypesAsync(Arg.Is(brandId));

        Assert.AreEqual(bucketTypes, response);
    }
}
