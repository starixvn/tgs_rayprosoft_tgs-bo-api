using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.ApplicationServices.Payment;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class BrandPaymentTests
{
    [Test]
    public async Task User_Can_List_Available_BrandPayments()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new BrandPaymentsQueryService(fakePaymentServiceClient, fakePlayerServiceClient);

        var brandId = Guid.NewGuid();
        var playerCode = "ThisIsPlayerCode";

        var query = new BrandPaymentsQuery(brandId, playerCode);

        _ = fakePaymentServiceClient
            .GetBrandPaymentAclsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Guid>>())
            .Returns(Array.Empty<BrandPaymentAcl>().ToAsyncEnumerable());

        var playerInfo = new Tgs.PlayerService.Client.PlayerInfo
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            IdentityAuthentication = false,
            PhoneAuthentication = false,
            VipLevelId = null
        };

        _ = fakePlayerServiceClient
            .GetPlayerInfoAsync(query.PlayerCode)
            .Returns(playerInfo);

        _ = await sut.QueryAsync(query).ToArrayAsync();

        _ = fakePaymentServiceClient
            .Received(1)
            .GetBrandPaymentAclsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<Guid>>());

        _ = fakePaymentServiceClient
            .Received(1)
            .FilterBrandPaymentsAsync(
                Arg.Any<IEnumerable<BrandPaymentInfo>>(),
                Arg.Any<Tgs.Payment.Clients.PlayerInfo>(),
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<IEnumerable<Guid>>(),
                Arg.Any<ILookup<Guid, Guid>>());
    }
}
