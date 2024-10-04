using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using ExpectedObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PlayerBucket.Repositories;
using StackExchange.Redis;
using Tgs.BrandCertificate.Clients;
using Tgs.CashService.Client;
using Tgs.Payment.Clients;
using Tgs.PlayerBucket.Client;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.Service.PlayerBuckets;
using TGS.Core.Infrastructures.Redis;
using TGS.Services.BackOffice.Application.Queries.Deposit;
using TGS.Services.BackOffice.ApplicationServices.Payment;
using Valhalla.MessageQueue;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class DepositFormTests
{
    [Test]
    public async Task Manager_Can_Get_The_DepositForm_Detail()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetDepositFormDetailQueryService(fakePaymentServiceClient);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var query = new GetDepositFormDetailQuery(formId, brandId);

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId
            });

        _ = await sut.GetAsync(query);

        _ = fakePaymentServiceClient.Received(1)
            .GetDepositFormAsync(Arg.Any<Guid>());
    }

    [Test]
    public async Task Manager_Can_Not_Review_DepositForm_When_Form_Status_Is_Approved()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo { Status = PaymentFlowState.Approved });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");
        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(0)
            .ReviewDepositFormAsync(
                brandId,
                ReviewState.Approve,
                "93a723b2-9ebb-4607-ee8d-08d978fba75f",
                "reviewUserName");
    }

    [Test]
    public async Task Manager_Can_Review_DepositForm()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>())
            .Returns(true);

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Id = Guid.NewGuid(),
                PaymentMethodId = Guid.NewGuid(),
                Amount = 1000M,
                BrandId = brandId,
                BrandPaymentId = Guid.NewGuid(),
                ChannelName = "ChannelName",
                CreatedOn = DateTimeOffset.Now,
                PlayerCode = "ThisIsPlayerCode",
                Status = PaymentFlowState.WaitApprove,
                UpdatedOn = DateTimeOffset.Now,
                UserName = "PlayerName"
            });

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Any<string>())
            .Returns(
                new Dictionary<string, string> { ["DefaultWalletId"] = walletId.ToString() }.ToImmutableDictionary());

        _ = fakeBrandServiceClient.GetBrandPaymentSettingsAsync(Arg.Is(brandId))
            .Returns(new BrandPaymentSettings(true, true, 0M));

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .ReviewDepositFormAsync(
                Arg.Is(formId),
                Arg.Is(ReviewState.Approve),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>());
    }

    [Test]
    public async Task User_Can_Get_Untreated_DepositForms_Count()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CountUntreatedDepositFormsQueryService(
            moqPaymentServiceClient,
            NullLogger<CountUntreatedDepositFormsQueryService>.Instance);

        var query = new CountUntreatedDepositFormsQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = moqPaymentServiceClient.Received(1).CountUntreatedDepositFormsAsync(
            Arg.Is<Guid>(x => x == query.BrandId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task User_Can_List_DepositForm()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new DepositFormQueryService(moqPaymentServiceClient);

        var query = new DepositFormQuery(
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            new());

        _ = await sut.GetAsync(query, default);

        _ = moqPaymentServiceClient.QueryDepositFormsAsync(
            Arg.Any<DepositFormQueryConditions>(),
            Arg.Any<Paging>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 審核存款單完成後會更新玩家總存款()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var playerCode = "test1";
        var walletId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Amount = 1000M,
                BrandPaymentId = brandPaymentId,
                PlayerCode = playerCode,
                BrandId = brandId,
                Status = PaymentFlowState.WaitApprove
            });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Is(new RedisKey($"Tgs:BucketDeposit:{command.FormId:N}")),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan>(),
                Arg.Is(When.NotExists))
            .Returns(true);

        _ = fakeBrandServiceClient.GetBrandPaymentSettingsAsync(Arg.Is(brandId))
            .Returns(new BrandPaymentSettings(
                false, false, 0M));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(
                new Dictionary<string, string> { ["DefaultWalletId"] = walletId.ToString() }.ToImmutableDictionary());

        _ = fakePaymentServiceClient.CalculateHandlingFeeAsync(Arg.Is(brandPaymentId), Arg.Is(1000M))
            .Returns(100M);

        await sut.ExecuteAsync(command);

        _ = fakePlayerServiceClient.Received(1)
            .TopUpAccumulatedDepositAsync(
                Arg.Is(playerCode),
                Arg.Is(900M));
    }

    [Test]
    public async Task 審核存款單沒有參加活動會進行錢包更新及加流水的動作()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var playerCode = "test1";
        var walletId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                Amount = 1000M,
                BrandPaymentId = brandPaymentId,
                PlayerCode = playerCode,
                BrandId = brandId,
                Status = PaymentFlowState.WaitApprove
            });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Is(new RedisKey($"Tgs:BucketDeposit:{command.FormId:N}")),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan>(),
                Arg.Is(When.NotExists))
            .Returns(true);

        _ = fakeBrandServiceClient.GetBrandPaymentSettingsAsync(Arg.Is(brandId))
            .Returns(new BrandPaymentSettings(
                false, false, 0M));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(
                new Dictionary<string, string> { ["DefaultWalletId"] = walletId.ToString() }.ToImmutableDictionary());

        _ = fakePaymentServiceClient.CalculateHandlingFeeAsync(Arg.Is(brandPaymentId), Arg.Is(1000M))
            .Returns(100M);

        await sut.ExecuteAsync(command);

        _ = fakeCashServiceClient.Received(1)
            .DepositedAsync(
                Arg.Is(walletId),
                Arg.Is((command.ActualAmount ?? 1000M) - 100M),
                Arg.Is(command.Remark),
                Arg.Is(formId.ToString()),
                Arg.Is(Guid.Parse(command.ReviewUserId)));
        _ = fakePlayerBucketClient.Received(1)
            .CreateBucketAsync(
                Arg.Is(playerCode),
                Arg.Is((command.ActualAmount ?? 1000M) - 100M),
                Arg.Is(BucketType.Deposit),
                Arg.Is(formId.ToString()),
                Arg.Is(command.Remark));
    }

    [Test]
    public async Task 審核存款單時如果平台有開自動清流水會進行清流水程序()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        _ = fakeRedisDatabase.StringSetAsync(
            Arg.Any<RedisKey>(),
            Arg.Any<RedisValue>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<When>(),
            Arg.Any<CommandFlags>()).Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var playerCode = "test1";
        var walletId = Guid.NewGuid();
        var promotionId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Amount = 1000M,
                BrandPaymentId = brandPaymentId,
                PlayerCode = playerCode,
                PromotionId = promotionId,
                Status = PaymentFlowState.WaitApprove
            });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Is(new RedisKey($"Tgs:BucketDeposit:{command.FormId:N}")),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan>(),
                Arg.Is(When.NotExists))
            .Returns(true);

        _ = fakeBrandServiceClient.GetBrandPaymentSettingsAsync(Arg.Is(brandId))
            .Returns(new BrandPaymentSettings(
                false, true, 0M));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(
                new Dictionary<string, string> { ["DefaultWalletId"] = walletId.ToString() }.ToImmutableDictionary());

        _ = fakePaymentServiceClient.CalculateHandlingFeeAsync(Arg.Is(brandPaymentId), Arg.Is(1000M))
            .Returns(100M);

        var bucketResult = new ReadBucketResult(
            1000M,
            Guid.NewGuid());

        _ = fakePlayerBucketClient.GetBrandBucketTypesAsync(Arg.Is(brandId))
            .Returns(AsyncEnumerable.Empty<string>());
        _ = fakePlayerBucketForTypesClient.ReadBucketsForBucketTypesAsync(
            Arg.Is(playerCode),
            Arg.Any<IEnumerable<string>>())
            .Returns(new[]
            {
                new TypeBucketResult(
                    null,
                    bucketResult)
            }.ToAsyncEnumerable());

        var expectedOrder = new CleanBucketOrder(
            brandId,
            walletId,
            0M,
            [
                new TypeBucketResult(
                    null,
                    bucketResult)
            ],
            formId,
            0M).ToExpectedObject(ctx => ctx
                .ArrayMemberShouldMatch(o => o.CurrentBucketAmounts)
                .IgnoreEqualsOverride());

        await sut.ExecuteAsync(command);

        _ = fakePlayerBucketClient.Received(1)
            .RequestCleanBucketsAsync(
                Arg.Is(playerCode),
                Arg.Is<CleanBucketOrder>(order => expectedOrder.Matches(order)));
    }

    [Test]
    public async Task 審核存款單時如果有其他審核程式正在處理中會結束執行不處理()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var playerCode = "test1";

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Amount = 1000M,
                BrandPaymentId = brandPaymentId,
                PlayerCode = playerCode,
                Status = PaymentFlowState.WaitApprove
            });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Is(new RedisKey($"Tgs:BucketDeposit:{command.FormId:N}")),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan>(),
                Arg.Is(When.NotExists))
            .Returns(false);

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.DidNotReceive()
            .SubmitDepositFormStatusAsync(
                Arg.Is(command.FormId),
                promotionPlayerId: Arg.Any<int?>(),
                handlingFee: Arg.Is(100M));
    }

    [Test]
    public async Task 審核存款單時會計算手續費()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
        var fakePlayerBucketClient = Substitute.For<IPlayerBucketClient>();
        var fakePlayerBucketForTypesClient = Substitute.For<IPlayerBucketForTypesClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakeCashServiceClient = Substitute.For<ICashServiceClient>();

        _ = fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        _ = fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient,
            fakePlayerBucketClient,
            fakePlayerBucketForTypesClient,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandServiceClient,
            fakeCashServiceClient,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var brandPaymentId = Guid.NewGuid();
        var playerCode = "test1";
        var walletId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId))
            .Returns(new DepositFormInfo
            {
                Amount = 1000M,
                BrandPaymentId = brandPaymentId,
                PlayerCode = playerCode,
                BrandId = brandId,
                Status = PaymentFlowState.WaitApprove
            });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Approve,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");

        _ = fakeRedisDatabase.StringSetAsync(
                Arg.Is(new RedisKey($"Tgs:BucketDeposit:{command.FormId:N}")),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan>(),
                Arg.Is(When.NotExists))
            .Returns(true);

        _ = fakeBrandServiceClient.GetBrandPaymentSettingsAsync(Arg.Is(brandId))
            .Returns(new BrandPaymentSettings(
                false, false, 0M));

        _ = fakePlayerServiceClient.GetPlayerPropertiesAsync(Arg.Is(playerCode))
            .Returns(
                new Dictionary<string, string> { ["DefaultWalletId"] = walletId.ToString() }.ToImmutableDictionary());

        _ = fakePaymentServiceClient.CalculateHandlingFeeAsync(Arg.Is(brandPaymentId), Arg.Is(1000M))
            .Returns(100M);

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(command.FormId),
                promotionPlayerId: Arg.Any<int?>(),
                handlingFee: Arg.Is(100M));
    }
}