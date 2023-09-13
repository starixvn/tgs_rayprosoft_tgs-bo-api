using NSubstitute;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Queries.PaymentMethod;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PaymentMethodTests
{
    [Test]
    public async Task User_Can_Create_PaymentMethod()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CreatePaymentMethodCommandService(moqPaymentServiceClient);

        var command = new CreatePaymentMethodCommand("MethodCode", "Bank");

        await sut.ExecuteAsync(command);

        _ = moqPaymentServiceClient.Received(1).CreatePaymentMethodAsync(
            Arg.Any<PaymentMethodCreateInfo>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void User_Can_Get_All_PaymentMethods()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new PaymentMethodQueryService(moqPaymentServiceClient);

        var query = new PaymentMethodQuery();

        _ = sut.QueryAsync(query);

        _ = moqPaymentServiceClient.Received(1).GetAllPaymentMethodsAsync(Arg.Any<CancellationToken>());
    }
}