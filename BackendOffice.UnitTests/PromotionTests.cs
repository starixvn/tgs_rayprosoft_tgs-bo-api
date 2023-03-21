using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;
using TGS.Services.BackOffice;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.ApplicationServices.Promotion;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PromotionTests
{
    [Test]
    public async Task Get_Promotion_Event_Detail()
    {
        var fakePromotionHistoryClient = Substitute.For<IPromotionHistoryClient>();

        var sut = new GetPromotionEventDetailQueryService(fakePromotionHistoryClient);

        var query = new GetPromotionEventDetailQuery(
            Guid.NewGuid(),
            Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakePromotionHistoryClient
            .Received(1)
            .GetPromotionEventDetailAsync(
                Arg.Is(query.PromotionId),
                Arg.Is(query.EventId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Get_Promotion_Events()
    {
        var fakePromotionHistoryClient = Substitute.For<IPromotionHistoryClient>();

        var sut = new GetPromotionEventsQueryService(fakePromotionHistoryClient);

        var query = new GetPromotionEventsQuery(Guid.NewGuid());

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePromotionHistoryClient
            .Received(1)
            .QueryPromotionEventsAsync(
                Arg.Is(query.PromotionId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Get_Promotion_Rule()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new GetPromotionRuleQueryService(fakePromotionServiceClient);

        var query = new GetPromotionRuleQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakePromotionServiceClient.Received(1)
            .GetPromotionRuleAsync<PromotionRuleInfo>(Arg.Is(query.PromotionId));
    }

    [Test]
    public async Task 取得活動MainInfo()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new GetPromotionMainInfoQueryService(fakePromotionServiceClient);

        var query = new GetPromotionMainInfoQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakePromotionServiceClient.Received(1)
            .GetPromotionMainInfoAsync(
                Arg.Is(query.PromotionId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 取得活動內容()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new GetPromotionContentQueryService(fakePromotionServiceClient);

        var query = new GetPromotionContentQuery(Guid.NewGuid());

        _ = await sut.GetAsync(query);

        _ = fakePromotionServiceClient.Received(1)
            .GetPromotionContentAsync(
                Arg.Is(query.PromotionId),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改活動MainInfo()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();

        var sut = new UpdatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeGetBrandInfoService);

        var command = new UpdatePromotionMainInfoCommand(
            Guid.NewGuid(),
            "TestPromotion",
            "TestPromotion",
            DateOnly.Parse("2023-01-01"),
            DateOnly.Parse("2023-03-01"),
            true,
            null,
            true,
            "User1");

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(Arg.Is(command.PromotionId))
            .Returns(new PromotionMainInfo
            {
                Id = command.PromotionId,
                BrandId = Guid.NewGuid()
            });

        var expectedCreateInfo = new CreatePromotionMainInfo
        {
            Name = "TestPromotion",
            EventDuring = new During(
                From: DateTime.Parse("2022-12-31 16:00:00"),
                To: DateTime.Parse("2023-02-28 16:00:00")),
            PromotionType = "TestPromotion",
            PromotionLimitAmount = null,
            IsAutoSent = command.IsAutoSent,
        };

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .UpdatePromotionMainInfoAsync(
                Arg.Is(command.PromotionId),
                Arg.Is(expectedCreateInfo),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());

        _ = fakeGetBrandInfoService.DidNotReceive()
            .GetAsync(
                Arg.Any<GetBrandInfoQuery>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改活動MainInfo_特定活動類型會套用Brand時區設定()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();

        var sut = new UpdatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeGetBrandInfoService);

        var promotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var command = new UpdatePromotionMainInfoCommand(
            promotionId,
            "TestPromotion",
            "TurnoverRebate",
            DateOnly.Parse("2023-01-01"),
            DateOnly.Parse("2023-03-01"),
            true,
            null,
            true,
            "User1");

        var promotionInfo = new PromotionMainInfo
        {
            Id = promotionId,
            BrandId = brandId,
            PromotionType = command.PromotionType
        };

        var brandInfo = new BrandInfo
        {
            Id = brandId,
            TimeSpanMinutes = 420
        };

        _ = fakePromotionServiceClient.GetPromotionMainInfoAsync(
                Arg.Is(promotionId),
                Arg.Any<CancellationToken>())
            .Returns(promotionInfo);

        _ = fakeGetBrandInfoService.GetAsync(
            Arg.Is(new GetBrandInfoQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var expectedCreateInfo = new CreatePromotionMainInfo
        {
            Name = "TestPromotion",
            EventDuring = new During(
                From: DateTime.Parse("2022-12-31 17:00:00"),
                To: DateTime.Parse("2023-02-28 17:00:00")),
            PromotionType = "TurnoverRebate",
            PromotionLimitAmount = null,
            IsAutoSent = command.IsAutoSent,
        };

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .UpdatePromotionMainInfoAsync(
                Arg.Is(command.PromotionId),
                Arg.Is(expectedCreateInfo),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());

        _ = fakeGetBrandInfoService.Received(1)
            .GetAsync(
                Arg.Is(new GetBrandInfoQuery(brandId)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改活動Rule()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new UpdatePromotionRuleCommandService(fakePromotionServiceClient);

        var promotionId = Guid.NewGuid();

        var command = new UpdatePromotionRuleCommand(
            promotionId,
            JsonDocument.Parse("{}"),
            new[] { 1, 2, 3 },
            "User1");

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionRuleAsync(
                Arg.Is(promotionId),
                Arg.Any<JsonDocument>(),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionVipLevelsAsync(
                Arg.Is(promotionId),
                Arg.Is<IEnumerable<int>>(x => x.SequenceEqual(command.VipLevels)),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改活動內容()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var sut = new UpdatePromotionContentCommandService(fakePromotionServiceClient);

        var command = new UpdatePromotionContentCommand(
            Guid.NewGuid(),
            "TestSubject",
            "TestContent",
            JsonDocument.Parse("{}"),
            null,
            null,
            null,
            "User1");

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionContentAsync(
                Arg.Is(command.PromotionId),
                Arg.Is<PromotionContent>(x => x.SubSubject == command.SubSubject),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改活動狀態()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

        var command = new UpdatePromotionStatusCommand(
            Guid.NewGuid(),
            PromotionStatus.Enabled,
            "User1");

        var sut = new UpdatePromotionStatusCommandService(fakePromotionServiceClient);

        await sut.ExecuteAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .SetPromotionStatusAsync(
                Arg.Is(command.PromotionId),
                Arg.Is(command.Status),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 新增活動MainInfo()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();

        var sut = new CreatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeGetBrandInfoService);

        var command = new CreatePromotionMainInfoCommand(
            Guid.NewGuid(),
            "TestPromotion",
            "TestPromotion",
            DateOnly.Parse("2023-01-01"),
            DateOnly.Parse("2023-03-01"),
            true,
            null,
            true,
            "User1");

        var expectedCreateInfo = new CreatePromotionMainInfo
        {
            Name = "TestPromotion",
            EventDuring = new During(
                From: DateTime.Parse("2022-12-31 16:00:00"),
                To: DateTime.Parse("2023-02-28 16:00:00")),
            PromotionType = "TestPromotion",
            PromotionLimitAmount = null,
            IsAutoSent = command.IsAutoSent,
        };

        _ = await sut.GetAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .CreatePromotionAsync(
                Arg.Is(command.BrandId),
                Arg.Is(expectedCreateInfo),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());

        _ = fakeGetBrandInfoService.DidNotReceive()
            .GetAsync(
                Arg.Is(new GetBrandInfoQuery(command.BrandId)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 新增活動MainInfo_特定活動類型會套用Brand時區設定()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();

        var sut = new CreatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeGetBrandInfoService);

        var brandId = Guid.NewGuid();

        var command = new CreatePromotionMainInfoCommand(
            brandId,
            "TestPromotion",
            "TurnoverRebate",
            DateOnly.Parse("2023-01-01"),
            DateOnly.Parse("2023-03-01"),
            true,
            null,
            true,
            "User1");

        var brandInfo = new BrandInfo
        {
            Id = brandId,
            TimeSpanMinutes = 420
        };

        _ = fakeGetBrandInfoService.GetAsync(
            Arg.Is(new GetBrandInfoQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(brandInfo);

        var expectedCreateInfo = new CreatePromotionMainInfo
        {
            Name = "TestPromotion",
            EventDuring = new During(
                From: DateTime.Parse("2022-12-31 17:00:00"),
                To: DateTime.Parse("2023-02-28 17:00:00")),
            PromotionType = "TurnoverRebate",
            PromotionLimitAmount = null,
            IsAutoSent = command.IsAutoSent,
        };

        _ = await sut.GetAsync(command);

        _ = fakePromotionServiceClient.Received(1)
            .CreatePromotionAsync(
                Arg.Is(command.BrandId),
                Arg.Is(expectedCreateInfo),
                Arg.Is(command.Modifier),
                Arg.Any<CancellationToken>());

        _ = fakeGetBrandInfoService.Received(1)
            .GetAsync(
                Arg.Is(new GetBrandInfoQuery(brandId)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 複製活動_活動結束時間大於目前時間()
    {
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();
        var fakeGetMainInfoService = Substitute.For<IGetService<GetPromotionMainInfoQuery, PromotionMainInfo>>();
        var fakeGetRuleService = Substitute.For<IGetService<GetPromotionRuleQuery, PromotionRuleInfo>>();
        var fakeGetContentService = Substitute.For<IGetService<GetPromotionContentQuery, PromotionContent>>();
        var fakeCreateMainInfoService = Substitute.For<IGetService<CreatePromotionMainInfoCommand, Guid>>();
        var fakeUpdateContentService = Substitute.For<ICommandService<UpdatePromotionContentCommand>>();
        var fakeUpdateRuleService = Substitute.For<ICommandService<UpdatePromotionRuleCommand>>();
        var fakeUpdateStatusService = Substitute.For<ICommandService<UpdatePromotionStatusCommand>>();
        var fakeLogger = NullLogger<DuplicatePromotionCommandService>.Instance;

        var sut = new DuplicatePromotionCommandService(
            fakeSystemClock,
            fakeGetBrandInfoService,
            fakeGetMainInfoService,
            fakeGetRuleService,
            fakeGetContentService,
            fakeCreateMainInfoService,
            fakeUpdateRuleService,
            fakeUpdateContentService,
            fakeUpdateStatusService,
            fakeLogger);

        var basePromotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var utcNow = DateTimeOffset.Parse("2023-03-16 16:00:00+00:00");

        var command = new DuplicatePromotionCommand(
            basePromotionId,
            "User1");

        var expectedPromotionId = Guid.NewGuid();

        _ = fakeSystemClock.UtcNow.Returns(utcNow);

        _ = fakeGetBrandInfoService.GetAsync(
            Arg.Is(new GetBrandInfoQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        _ = fakeGetMainInfoService.GetAsync(
            Arg.Is(new GetPromotionMainInfoQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = basePromotionId,
                BrandId = brandId,
                Name = "TestPromotion",
                EventDuring = new During(
                    DateTime.Parse("2023-03-15 16:00:00"),
                    DateTime.Parse("2023-03-17 16:00:00")),
                IsAutoSent = false,
                PromotionLimitAmount = null,
                PromotionType = "TestPromotion"
            });

        _ = fakeGetRuleService.GetAsync(
            Arg.Is(new GetPromotionRuleQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionRuleInfo(
                JsonDocument.Parse("{}"),
                1000m,
                10m,
                1m,
                null,
                new[] { 1, 2, 3 }));

        _ = fakeGetContentService.GetAsync(
            Arg.Is(new GetPromotionContentQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionContent
            {
                SubSubject = "TestSubject",
                Localization = JsonDocument.Parse("{}")
            });

        var createInfoCommand = new CreatePromotionMainInfoCommand(
            brandId,
            "TestPromotion",
            "TestPromotion",
            DateOnly.Parse("2023-03-18"),
            DateOnly.Parse("2023-03-20"),
            true,
            null,
            false,
            command.Modifier);

        _ = fakeCreateMainInfoService.GetAsync(
            Arg.Is(createInfoCommand),
            Arg.Any<CancellationToken>())
            .Returns(expectedPromotionId);

        var actual = await sut.GetAsync(command);

        Assert.AreEqual(expectedPromotionId, actual);

        _ = fakeUpdateRuleService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionRuleCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateContentService.Received(1)
             .ExecuteAsync(
                 Arg.Is<UpdatePromotionContentCommand>(x => x.PromotionId == expectedPromotionId),
                 Arg.Any<CancellationToken>());

        _ = fakeUpdateStatusService.Received(1)
            .ExecuteAsync(
                Arg.Is(new UpdatePromotionStatusCommand(
                    expectedPromotionId,
                    PromotionStatus.Disabled,
                    command.Modifier)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 複製活動_活動結束時間小於目前時間()
    {
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();
        var fakeGetMainInfoService = Substitute.For<IGetService<GetPromotionMainInfoQuery, PromotionMainInfo>>();
        var fakeGetRuleService = Substitute.For<IGetService<GetPromotionRuleQuery, PromotionRuleInfo>>();
        var fakeGetContentService = Substitute.For<IGetService<GetPromotionContentQuery, PromotionContent>>();
        var fakeCreateMainInfoService = Substitute.For<IGetService<CreatePromotionMainInfoCommand, Guid>>();
        var fakeUpdateContentService = Substitute.For<ICommandService<UpdatePromotionContentCommand>>();
        var fakeUpdateRuleService = Substitute.For<ICommandService<UpdatePromotionRuleCommand>>();
        var fakeUpdateStatusService = Substitute.For<ICommandService<UpdatePromotionStatusCommand>>();
        var fakeLogger = NullLogger<DuplicatePromotionCommandService>.Instance;

        var sut = new DuplicatePromotionCommandService(
            fakeSystemClock,
            fakeGetBrandInfoService,
            fakeGetMainInfoService,
            fakeGetRuleService,
            fakeGetContentService,
            fakeCreateMainInfoService,
            fakeUpdateRuleService,
            fakeUpdateContentService,
            fakeUpdateStatusService,
            fakeLogger);

        var basePromotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var utcNow = DateTimeOffset.Parse("2023-03-16 16:00:00+00:00");

        var command = new DuplicatePromotionCommand(
            basePromotionId,
            "User1");

        var expectedPromotionId = Guid.NewGuid();

        _ = fakeSystemClock.UtcNow.Returns(utcNow);

        _ = fakeGetBrandInfoService.GetAsync(
            Arg.Is(new GetBrandInfoQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 480
            });

        _ = fakeGetMainInfoService.GetAsync(
            Arg.Is(new GetPromotionMainInfoQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = basePromotionId,
                BrandId = brandId,
                Name = "TestPromotion",
                EventDuring = new During(
                    DateTime.Parse("2023-03-13 16:00:00"),
                    DateTime.Parse("2023-03-15 16:00:00")),
                IsAutoSent = false,
                PromotionLimitAmount = null,
                PromotionType = "TestPromotion"
            });

        _ = fakeGetRuleService.GetAsync(
            Arg.Is(new GetPromotionRuleQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionRuleInfo(
                JsonDocument.Parse("{}"),
                1000m,
                10m,
                1m,
                null,
                new[] { 1, 2, 3 }));

        _ = fakeGetContentService.GetAsync(
            Arg.Is(new GetPromotionContentQuery(basePromotionId)),
            Arg.Any<CancellationToken>()).Returns(new PromotionContent
            {
                SubSubject = "TestSubject",
                Localization = JsonDocument.Parse("{}")
            });

        var createInfoCommand = new CreatePromotionMainInfoCommand(
            brandId,
            "TestPromotion",
            "TestPromotion",
            DateOnly.Parse("2023-03-18"),
            DateOnly.Parse("2023-03-20"),
            true,
            null,
            false,
            command.Modifier);

        _ = fakeCreateMainInfoService.GetAsync(
            Arg.Is(createInfoCommand),
            Arg.Any<CancellationToken>())
            .Returns(expectedPromotionId);

        var actual = await sut.GetAsync(command);

        Assert.AreEqual(expectedPromotionId, actual);

        _ = fakeGetBrandInfoService.DidNotReceive()
            .GetAsync(
                Arg.Is(new GetBrandInfoQuery(brandId)),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateRuleService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionRuleCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateContentService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionContentCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateStatusService.Received(1)
            .ExecuteAsync(
                Arg.Is(new UpdatePromotionStatusCommand(
                    expectedPromotionId,
                    PromotionStatus.Disabled,
                    command.Modifier)),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 複製活動_活動結束時間小於目前時間_特定活動類型會套用Brand時區設定()
    {
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeGetBrandInfoService = Substitute.For<IGetService<GetBrandInfoQuery, BrandInfo?>>();
        var fakeGetMainInfoService = Substitute.For<IGetService<GetPromotionMainInfoQuery, PromotionMainInfo>>();
        var fakeGetRuleService = Substitute.For<IGetService<GetPromotionRuleQuery, PromotionRuleInfo>>();
        var fakeGetContentService = Substitute.For<IGetService<GetPromotionContentQuery, PromotionContent>>();
        var fakeCreateMainInfoService = Substitute.For<IGetService<CreatePromotionMainInfoCommand, Guid>>();
        var fakeUpdateContentService = Substitute.For<ICommandService<UpdatePromotionContentCommand>>();
        var fakeUpdateRuleService = Substitute.For<ICommandService<UpdatePromotionRuleCommand>>();
        var fakeUpdateStatusService = Substitute.For<ICommandService<UpdatePromotionStatusCommand>>();
        var fakeLogger = NullLogger<DuplicatePromotionCommandService>.Instance;

        var sut = new DuplicatePromotionCommandService(
            fakeSystemClock,
            fakeGetBrandInfoService,
            fakeGetMainInfoService,
            fakeGetRuleService,
            fakeGetContentService,
            fakeCreateMainInfoService,
            fakeUpdateRuleService,
            fakeUpdateContentService,
            fakeUpdateStatusService,
            fakeLogger);

        var basePromotionId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var utcNow = DateTimeOffset.Parse("2023-03-16 17:00:00+00:00");

        var command = new DuplicatePromotionCommand(
            basePromotionId,
            "User1");

        var expectedPromotionId = Guid.NewGuid();

        _ = fakeSystemClock.UtcNow.Returns(utcNow);

        _ = fakeGetBrandInfoService.GetAsync(
            Arg.Is(new GetBrandInfoQuery(brandId)),
            Arg.Any<CancellationToken>())
            .Returns(new BrandInfo
            {
                Id = brandId,
                TimeSpanMinutes = 420
            });

        _ = fakeGetMainInfoService.GetAsync(
            Arg.Is(new GetPromotionMainInfoQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionMainInfo
            {
                Id = basePromotionId,
                BrandId = brandId,
                Name = "TestPromotion",
                EventDuring = new During(
                    DateTime.Parse("2023-03-13 17:00:00"),
                    DateTime.Parse("2023-03-15 17:00:00")),
                IsAutoSent = false,
                PromotionLimitAmount = null,
                PromotionType = "TurnoverRebate"
            });

        _ = fakeGetRuleService.GetAsync(
            Arg.Is(new GetPromotionRuleQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionRuleInfo(
                JsonDocument.Parse("{}"),
                1000m,
                10m,
                1m,
                null,
                new[] { 1, 2, 3 }));

        _ = fakeGetContentService.GetAsync(
            Arg.Is(new GetPromotionContentQuery(basePromotionId)),
            Arg.Any<CancellationToken>())
            .Returns(new PromotionContent
            {
                SubSubject = "TestSubject",
                Localization = JsonDocument.Parse("{}")
            });

        var createInfoCommand = new CreatePromotionMainInfoCommand(
            brandId,
            "TestPromotion",
            "TurnoverRebate",
            DateOnly.Parse("2023-03-18"),
            DateOnly.Parse("2023-03-20"),
            true,
            null,
            false,
            command.Modifier);

        _ = fakeCreateMainInfoService.GetAsync(
            Arg.Is(createInfoCommand),
            Arg.Any<CancellationToken>())
            .Returns(expectedPromotionId);

        var actual = await sut.GetAsync(command);

        Assert.AreEqual(expectedPromotionId, actual);

        _ = fakeGetBrandInfoService.Received(1)
            .GetAsync(
                Arg.Is(new GetBrandInfoQuery(brandId)),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateRuleService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionRuleCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateContentService.Received(1)
            .ExecuteAsync(
                Arg.Is<UpdatePromotionContentCommand>(x => x.PromotionId == expectedPromotionId),
                Arg.Any<CancellationToken>());

        _ = fakeUpdateStatusService.Received(1)
            .ExecuteAsync(
                Arg.Is(new UpdatePromotionStatusCommand(
                    expectedPromotionId,
                    PromotionStatus.Disabled,
                    command.Modifier)),
                Arg.Any<CancellationToken>());
    }
}
