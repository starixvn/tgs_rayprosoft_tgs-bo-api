using System.Text.Json;
using NSubstitute;
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
}
