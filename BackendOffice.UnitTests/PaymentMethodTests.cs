using Moq;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Queries.PaymentMethod;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PaymentMethodTests
{
    [Test]
    public async Task User_Can_Create_PaymentMethod()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new CreatePaymentMethodCommandService(moqPaymentServiceClient.Object);

        var command = new CreatePaymentMethodCommand("MethodCode", "Bank");

        await sut.ExecuteAsync(command);

        moqPaymentServiceClient.Verify(
            fake => fake.CreatePaymentMethodAsync(
                It.IsAny<PaymentMethodCreateInfo>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Test]
    public void User_Can_Get_All_PaymentMethods()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new PaymentMethodQueryService(moqPaymentServiceClient.Object);

        var query = new PaymentMethodQuery();

        _ = sut.QueryAsync(query);

        moqPaymentServiceClient.Verify(
            fake => fake.GetAllPaymentMethodsAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }
}
