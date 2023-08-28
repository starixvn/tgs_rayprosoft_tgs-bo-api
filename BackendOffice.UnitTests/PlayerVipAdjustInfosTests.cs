using NSubstitute;
using Tgs.VipLevelService.Clients;
using TGS.Services.BackOffice.ApplicationServices.Players;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerVipAdjustInfosTests
{
    [Test]
    public async Task GetPlayerVipAdjustQueryService()
    {
        var fakeClient = Substitute.For<IVipLevelServiceClient>();
        var sut = new GetPlayerVipAdjustQueryService(fakeClient);
        _ = await sut.GetAsync(new GetPlayerVipAdjustQuery(
            "playerCode",
            Guid.NewGuid(),
            DateTimeOffset.Now.AddDays(-10),
            DateTimeOffset.Now,
            new Core.Models.Messages.PageableRequest
            {
                Page = 1,
                Rows = 20,
            }));

        _ = fakeClient.Received(1)
            .QueryPlayerVipAdjustInfosAsync(Arg.Any<PlayerVipAdjustQueryConditions>(), Arg.Any<Paging>());
    }
}
