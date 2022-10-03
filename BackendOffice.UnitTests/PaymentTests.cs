using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
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
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new CreatePaymentCommandService(moqPaymentServiceClient.Object);

        var command = new CreatePaymentCommand("Test", null, null);

        await sut.ExecuteAsync(command);

        moqPaymentServiceClient.Verify(
            fake => fake.CreatePaymentAsync(
                It.IsAny<PaymentCreateInfo>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Test]
    public void Can_Get_All_Payments()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new AllPaymentQueryService(moqPaymentServiceClient.Object);

        sut.QueryAsync(new AllPaymentQuery());

        moqPaymentServiceClient.Verify(
            fake => fake.GetAllPaymentsAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }
}
