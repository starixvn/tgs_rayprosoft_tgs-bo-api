using NSubstitute;
using Tgs.VipLevelService.Clients;
using TGS.Services.BackOffice.ApplicationServices.Players;
using TGS.Services.BackOffice.Domain;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerVipAdjustInfosTests
{
    [Test]
    public async Task GetPlayerVipAdjustQueryService()
    {
        var fakeClient = Substitute.For<IVipLevelServiceClient>();

        var query = new GetPlayerVipAdjustQuery(
            PlayerCode: "playerCode",
            BrandId: Guid.NewGuid(),
            From: DateTimeOffset.Now.AddDays(-10),
            To: DateTimeOffset.Now,
            Paging: new PageableRequest
            {
                Page = 1,
                Rows = 20,
            });

        var sut = new GetPlayerVipAdjustQueryService(fakeClient);
        _ = await sut.GetAsync(query);

        _ = fakeClient.Received(1)
            .PlayerVipLevelLogQueryAsync(
            playerCode: Arg.Is(query.PlayerCode),
            startOn: Arg.Is(query.From),
            endOn: Arg.Is(query.To),
            page: Arg.Is(query.Paging.Page),
            rowCount: Arg.Is(query.Paging.Rows),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
