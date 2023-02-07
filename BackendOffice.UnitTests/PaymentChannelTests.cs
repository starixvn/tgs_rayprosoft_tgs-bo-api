using Moq;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Queries.PaymentChannel;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PaymentChannelTests
{
    [Test]
    public async Task User_Can_Create_PaymentChannel()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new CreatePaymentChannelCommandService(moqPaymentServiceClient.Object);

        var command = new CreatePaymentChannelCommand("ChannelCode");

        await sut.ExecuteAsync(command);

        moqPaymentServiceClient.Verify(
            fake => fake.CreatePaymentChannelAsync(
                command.Code,
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Test]
    public void User_Can_Get_All_PaymentChannels()
    {
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();

        var sut = new PaymentChannelQueryService(moqPaymentServiceClient.Object);

        _ = sut.QueryAsync(new PaymentChannelQuery());

        moqPaymentServiceClient.Verify(
            fake => fake.GetAllPaymentChannelsAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }
}
