using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Queries.BrandPayment;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
[Parallelizable]
public class PaymentTests
{
    [Test]
    public async Task Can_Create_Payment()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CreatePaymentCommandService(moqPaymentServiceClient);

        var command = new CreatePaymentCommand("Test", null, null);

        await sut.ExecuteAsync(command);

        _ = moqPaymentServiceClient.Received(1).CreatePaymentAsync(
            Arg.Any<PaymentCreateInfo>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void Can_Get_All_Payments()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new AllPaymentQueryService(moqPaymentServiceClient);

        _ = sut.QueryAsync(new());

        _ = moqPaymentServiceClient.Received(1).GetAllPaymentsAsync(Arg.Any<CancellationToken>());
    }
}