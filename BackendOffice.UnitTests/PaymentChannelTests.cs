using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Queries.PaymentChannel;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PaymentChannelTests
{
    [Test]
    public async Task User_Can_Create_PaymentChannel()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CreatePaymentChannelCommandService(moqPaymentServiceClient);

        var command = new CreatePaymentChannelCommand("ChannelCode");

        await sut.ExecuteAsync(command);

        _ = moqPaymentServiceClient.Received(1).CreatePaymentChannelAsync(
            command.Code,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void User_Can_Get_All_PaymentChannels()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new PaymentChannelQueryService(moqPaymentServiceClient);

        _ = sut.QueryAsync(new());

        _ = moqPaymentServiceClient.Received(1).GetAllPaymentChannelsAsync(Arg.Any<CancellationToken>());
    }
}