using System.Text.Json;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.Promotion.Clients;
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
    public async Task 修改活動MainInfo()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new UpdatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeBrandServiceClient);

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

        _ = fakeBrandServiceClient.DidNotReceive()
            .GetBrandInfoAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 修改活動MainInfo_特定活動類型會取得Brand時區設定()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new UpdatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeBrandServiceClient);

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

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
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

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandInfoAsync(
                Arg.Is(brandId),
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
    public async Task 新增活動MainInfo()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new CreatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeBrandServiceClient);

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

        _ = fakeBrandServiceClient.DidNotReceive()
            .GetBrandInfoAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task 新增活動MainInfo_特定活動類型會取得Brand時區設定()
    {
        var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();

        var sut = new CreatePromotionMainInfoCommandService(fakePromotionServiceClient, fakeBrandServiceClient);

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

        _ = fakeBrandServiceClient.GetBrandInfoAsync(
                Arg.Is(brandId),
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

        _ = fakeBrandServiceClient.Received(1)
            .GetBrandInfoAsync(
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}
