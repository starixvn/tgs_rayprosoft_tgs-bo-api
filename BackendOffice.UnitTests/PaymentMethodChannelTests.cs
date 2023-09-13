using NSubstitute;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Commands.PaymentMethodChannel;
using TGS.Services.BackOffice.Application.Queries.BrandPayment;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class PaymentMethodChannelTests
{
    [Test]
    public async Task Can_Create_PaymentMethodChannel_Data()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CreatePaymentMethodChannelCommandService(fakePaymentServiceClient);

        var command = new CreatePaymentMethodChannelCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .CreatePaymentMethodChannelAsync(
                Arg.Is(command.PaymentId),
                Arg.Is(command.MethodId),
                Arg.Is(command.ChannelId));
    }

    [Test]
    public void Can_Delete_PaymentMethodChannel_Data()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new DeletePaymentMethodChannelCommandService(fakePaymentServiceClient);

        var command = new DeletePaymentMethodChannelCommand(Guid.NewGuid());

        _ = sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .DeletePaymentMethodChannelAsync(Arg.Is(command.Id));
    }

    [Test]
    public void Should_Get_All_PaymentMethodChannels()
    {
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new AllPaymentMethodChannelQueryService(moqPaymentServiceClient);

        _ = sut.QueryAsync(new());

        _ = moqPaymentServiceClient.Received(1).GetAllPaymentMethodChannelsAsync(Arg.Any<CancellationToken>());
    }
}