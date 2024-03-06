using Tgs.Payment.Clients;
using TGS.Services.BackOffice.ApplicationServices.Payment;

namespace TGS.Tests.BackendOffice.UnitTests;

public class PlayerPaymentTests
{
    [Test]
    public async Task 取得玩家金流總覽()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new GetPlayerPaymentOverviewQueryService(fakePaymentServiceClient);

        var query = new GetPlayerPaymentOverviewQuery("test1");

        _ = fakePaymentServiceClient.GetPlayerPaymentOverviewAsync(Arg.Is(query.PlayerCode))
            .Returns(new PaymentOverview(
                0,
                0,
                DateTime.UtcNow,
                0,
                0,
                DateTime.UtcNow,
                DateOnly.FromDateTime(DateTime.Now),
                0M,
                0,
                0M,
                0));

        _ = await sut.GetAsync(query);

        _ = fakePaymentServiceClient.Received(1)
            .GetPlayerPaymentOverviewAsync(Arg.Is(query.PlayerCode));
    }
}
