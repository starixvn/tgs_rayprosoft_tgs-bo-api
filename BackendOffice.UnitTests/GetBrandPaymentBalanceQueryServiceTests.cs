using Tgs.Payment.Clients;
using TGS.Services.BackOffice.ApplicationServices.Payment;

namespace TGS.Tests.BackendOffice.UnitTests;

public class GetBrandPaymentBalanceQueryServiceTests
{
    [Test]
    public async Task GetBrandPaymentBalanceQueryService_依照BrandId及BrandPaymentId取得商戶餘額()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var sut = new GetBrandPaymentBalanceQueryService(fakePaymentServiceClient, fakePaymentClient);

        var query = new GetBrandPaymentBalanceQuery(Guid.NewGuid(), Guid.NewGuid());
        var paymentId = Guid.NewGuid();
        var paymentCode = "Test";
        var brandPaymentExtendData = "{}";

        _ = fakePaymentServiceClient.GetBrandPaymentByIdAsync(Arg.Is(query.BrandPaymentId))
            .Returns(new BrandPaymentInfo
            {
                Id = query.BrandPaymentId,
                BrandId = query.BrandId,
                ExtendedData = brandPaymentExtendData,
                PaymentId = paymentId
            });

        _ = fakePaymentServiceClient.GetPaymentByIdAsync(Arg.Is(paymentId))
            .Returns(new PaymentInfo
            {
                Id = paymentId,
                Code = paymentCode
            });

        _ = await sut.GetAsync(query);

        _ = fakePaymentClient.Received(1)
            .GetMerchantBalanceAsync(
                Arg.Is(paymentCode),
                Arg.Is(brandPaymentExtendData));
    }

    [Test]
    public async Task GetBrandPaymentBalanceQueryService_取得商戶餘額_輸入的BrandPayment不存在_回傳Null()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var sut = new GetBrandPaymentBalanceQueryService(fakePaymentServiceClient, fakePaymentClient);

        var query = new GetBrandPaymentBalanceQuery(Guid.NewGuid(), Guid.NewGuid());

        var actual = await sut.GetAsync(query);

        Assert.That(actual, Is.Null);
    }

    [Test]
    public async Task GetBrandPaymentBalanceQueryService_取得商戶餘額_取得的BrandPayment和要求不同時_回傳Null()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        var sut = new GetBrandPaymentBalanceQueryService(fakePaymentServiceClient, fakePaymentClient);

        var query = new GetBrandPaymentBalanceQuery(Guid.NewGuid(), Guid.NewGuid());
        var brandPaymentExtendData = "{}";

        _ = fakePaymentServiceClient.GetBrandPaymentByIdAsync(Arg.Is(query.BrandPaymentId))
            .Returns(new BrandPaymentInfo
            {
                Id = query.BrandPaymentId,
                BrandId = Guid.NewGuid(),
                ExtendedData = brandPaymentExtendData,
                PaymentId = Guid.NewGuid()
            });

        var actual = await sut.GetAsync(query);

        Assert.That(actual, Is.Null);
    }
}
