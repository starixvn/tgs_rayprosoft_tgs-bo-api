using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NSubstitute;
using NUnit.Framework;
using StackExchange.Redis;
using Tgs.Payment.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Redis;
using TGS.Services.BackOffice.Application.Models.Player;
using TGS.Services.BackOffice.Application.Queries.Deposit;
using TGS.Services.BackOffice.Application.Queries.PlayerHandlers;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Infrastructure.DataService;
using TGS.Services.PlayerBucket;
using TGS.Services.PlayerBucket.Models;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class DepositFormTests
{
    [Test]
    public async Task Manager_Can_Get_The_DepositForm_Detail()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new GetDepositFormDetailQueryService(moqPaymentServiceClient.Object);

        var formId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var query = new GetDepositFormDetailQuery(formId, brandId);

        await sut.GetAsync(query);

        moqPaymentServiceClient.Verify(
            fake => fake.GetDepositFormAsync(
                It.Is<Guid>(x => x == brandId),
                It.Is<Guid>(x => x == formId),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Test]
    public async Task Manager_Can_Not_Review_DepositForm_When_Form_Status_Is_Approved()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeMediator = Substitute.For<IMediator>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakePlayerGPBalanceService = Substitute
            .For<IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse>>();

        fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerService,
            fakePlayerBucketRepository,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandService,
            fakePlayerGPBalanceService,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();

        fakePaymentServiceClient.GetDepositFormAsync(brandId, formId)
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

    public async Task Manager_Can_Not_Review_DepositForm_When_Form_Status_Is_Reject()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakePlayerBucketRepository = Substitute.For<IPlayerBucketRepository>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeMediator = Substitute.For<IMediator>();
        var fakeBrandService = Substitute.For<IBrandService>();
        var fakePlayerGPBalanceService = Substitute
            .For<IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse>>();

        fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            fakePaymentServiceClient,
            fakePlayerService,
            fakePlayerBucketRepository,
            fakePromotionClient,
            fakeRedisDatabaseFactory,
            fakeBrandService,
            fakePlayerGPBalanceService,
            NullLogger<ReviewDepositFormCommandService>.Instance);

        fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        var brandId = Guid.NewGuid();
        var formId = Guid.NewGuid();

        fakePaymentServiceClient.GetDepositFormAsync(brandId, formId)
            .Returns(new DepositFormInfo { Status = PaymentFlowState.Reject });

        var command = new ReviewDepositFormCommand(
            brandId,
            formId,
            ReviewState.Reject,
            null,
            null,
            "93a723b2-9ebb-4607-ee8d-08d978fba75f",
            "reviewUserName");
        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(0)
            .ReviewDepositFormAsync(
                brandId,
                ReviewState.Reject,
                "93a723b2-9ebb-4607-ee8d-08d978fba75f",
                "reviewUserName");
    }

    [Test]
    public async Task Manager_Can_Review_DepositForm()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();
        var moqPlayerService = new Mock<IPlayerService>();
        var moqPlayerBucketRepository = new Mock<IPlayerBucketRepository>();
        var moqPromotionClient = new Mock<IPromotionClient>();
        var fakeRedisDatabase = Substitute.For<IDatabase>();
        var fakeRedisConnection = Substitute.For<IConnectionMultiplexer>();
        var fakeRedisDatabaseFactory = Substitute.For<IRedisConnectionFactory>();
        var fakeMediator = Substitute.For<IMediator>();
        var moqBrandService = new Mock<IBrandService>();
        var fakePlayerGPBalanceService = Substitute
            .For<IQueryService<GetPlayerGameProviderBalanceCommand, GetPlayerGameProviderBalanceResponse>>();

        fakeRedisDatabaseFactory.GetConnection()
            .Returns(fakeRedisConnection);

        fakeRedisConnection.GetDatabase(Arg.Any<int>(), Arg.Any<object>())
            .Returns(fakeRedisDatabase);

        var sut = new ReviewDepositFormCommandService(
            moqPaymentServiceClient.Object,
            moqPlayerService.Object,
            moqPlayerBucketRepository.Object,
            moqPromotionClient.Object,
            fakeRedisDatabaseFactory,
            moqBrandService.Object,
            fakePlayerGPBalanceService,
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

        fakeRedisDatabase.StringSetAsync(
                Arg.Any<RedisKey>(),
                Arg.Any<RedisValue>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<bool>(),
                Arg.Any<When>(),
                Arg.Any<CommandFlags>())
            .Returns(true);

        moqPaymentServiceClient
            .Setup(fake => fake.GetDepositFormAsync(
                It.Is<Guid>(x => x == brandId),
                It.Is<Guid>(x => x == formId),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<DepositFormInfo?>(new DepositFormInfo
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
            }));

        moqPlayerService
            .Setup(fake => fake.GetPlayerAsync(
                It.IsAny<Expression<Func<Player, bool>>>(),
                It.IsAny<Func<IQueryable<Player>, IQueryable<Player>>>()))
            .Returns(Task.FromResult(new Player
            {
                PlayerCode = "ThisIsPlayerCode",
                DefaultWalletId = walletId
            }));

        moqBrandService
            .Setup(fake => fake.GetBrandAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<Func<IQueryable<Brand>, IQueryable<Brand>>>()))
            .Returns(Task.FromResult(new Brand { AutoClearWageringRequirement = false }));

        moqPlayerBucketRepository
            .Setup(fake => fake.CreatePlayerBucketWithDepositAsync(
                It.Is<CreatePlayerBucketWithDepositRequest>(
                    x => x.BrandId == brandId
                        && x.DefaultWalletId == walletId
                        && x.DepositAmount == 1000M
                        && x.PlayerCode == "ThisIsPlayerCode")))
            .Returns(ValueTask.FromResult(
                new CreatePlayerBucketWithDepositResponse(1000M, null)));

        await sut.ExecuteAsync(command);

        moqPaymentServiceClient.Verify(
            fake => fake.ReviewDepositFormAsync(
                It.Is<Guid>(x => x == formId),
                It.Is<ReviewState>(x => x == ReviewState.Approve),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        moqPlayerBucketRepository.Verify(
            fake => fake.CreatePlayerBucketWithDepositAsync(
                It.Is<CreatePlayerBucketWithDepositRequest>(
                    x => x.BrandId == brandId
                        && x.DefaultWalletId == walletId
                        && x.DepositAmount == 1000M
                        && x.PlayerCode == "ThisIsPlayerCode")),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_Get_Untreated_DepositForms_Count()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new CountUntreatedDepositFormsQueryService(moqPaymentServiceClient.Object);

        var query = new CountUntreatedDepositFormsQuery(Guid.NewGuid());

        await sut.GetAsync(query);

        moqPaymentServiceClient.Verify(
            fake => fake.CountUntreatedDepositFormsAsync(
                It.Is<Guid>(x => x == query.BrandId),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Test]
    public async Task User_Can_List_DepositForm()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new DepositFormQueryService(moqPaymentServiceClient.Object);

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
            new Paging());

        _ = await sut.GetAsync(query, default);

        moqPaymentServiceClient.Verify(
            fake => fake.QueryDepositFormsAsync(
                It.IsAny<DepositFormQueryConditions>(),
                It.IsAny<Paging>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }
}
