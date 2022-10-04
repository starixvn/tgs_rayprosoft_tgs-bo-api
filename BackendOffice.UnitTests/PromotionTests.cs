using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Promotion.Clients;
using TGS.Services.BackOffice.ApplicationServices;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PromotionTests
{
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
