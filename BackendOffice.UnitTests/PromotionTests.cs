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
}
