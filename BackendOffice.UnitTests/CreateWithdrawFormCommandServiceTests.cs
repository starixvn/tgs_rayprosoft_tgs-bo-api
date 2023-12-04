using ExpectedObjects;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tgs.Payment.Clients;
using Tgs.PlayerService.Client;
using TGS.Services.BackOffice.Application.Commands.Withdraw;

namespace TGS.Tests.BackendOffice.UnitTests;

public class CreateWithdrawFormCommandServiceTests
{
    [Test]
    public async Task 檢查提款申請BankInfo應該存在()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CheckWithdrawFormCanCreateQueryService(fakePaymentServiceClient);

        var command = new CreateWithdrawFormCommand(
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            100M,
            null);

        _ = fakePaymentServiceClient.GetPlayerBankInfosAsync(Arg.Is(command.PlayerCode))
            .Returns(AsyncEnumerable.Empty<PlayerBankInfo>());

        var actual = await sut.GetAsync(command);

        ClassicAssert.IsFalse(actual);
    }

    [Test]
    public async Task 檢查提款申請金額應該不為零()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CheckWithdrawFormCanCreateQueryService(fakePaymentServiceClient);

        var command = new CreateWithdrawFormCommand(
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            0M,
            null);

        var actual = await sut.GetAsync(command);

        ClassicAssert.IsFalse(actual);
    }

    [Test]
    public async Task 檢查提款申請滿足條件會回傳true()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CheckWithdrawFormCanCreateQueryService(fakePaymentServiceClient);

        var command = new CreateWithdrawFormCommand(
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            100M,
            null);

        var paymentChannelId = Guid.NewGuid();

        _ = fakePaymentServiceClient.GetPlayerBankInfosAsync(Arg.Is(command.PlayerCode))
            .Returns(new[]
            {
                new PlayerBankInfo
                {
                    Id = command.BankInfoId,
                    PaymentChannelId = paymentChannelId
                }
            }.ToAsyncEnumerable());

        _ = fakePaymentServiceClient.GetBrandPaymentsAsync(Arg.Is(command.BrandId))
            .Returns(new[]
            {
                new BrandPaymentInfo
                {
                    Id = Guid.NewGuid(),
                    ChannelId = paymentChannelId
                }
            }.ToAsyncEnumerable());

        var actual = await sut.GetAsync(command);

        ClassicAssert.IsTrue(actual);
    }

    [Test]
    public async Task 檢查提款申請需要有一個以上的BrandPayment滿足提款條件()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CheckWithdrawFormCanCreateQueryService(fakePaymentServiceClient);

        var command = new CreateWithdrawFormCommand(
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            100M,
            null);

        _ = fakePaymentServiceClient.GetPlayerBankInfosAsync(Arg.Is(command.PlayerCode))
            .Returns(new[]
            {
                new PlayerBankInfo
                {
                    Id = command.BankInfoId
                }
            }.ToAsyncEnumerable());

        var actual = await sut.GetAsync(command);

        ClassicAssert.IsFalse(actual);
    }

    [Test]
    public void 建立提款單時指定的玩家找不到會發生Exception()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new CreateWithdrawFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient);

        var command = new CreateWithdrawFormCommand(
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            100M,
            null);

        _ = Assert.ThrowsAsync<Exception>(async () => await sut.ExecuteAsync(command));
    }

    [Test]
    public async Task 建立提款單()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

        var sut = new CreateWithdrawFormCommandService(
            fakePaymentServiceClient,
            fakePlayerServiceClient);

        var command = new CreateWithdrawFormCommand(
            "test1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            100M,
            null);

        _ = fakePlayerServiceClient.GetPlayerInfoAsync(Arg.Is(command.PlayerCode))
            .Returns(new Tgs.PlayerService.Client.PlayerInfo
            {
                PlayerCode = command.PlayerCode,
                UserName = "user1",
                BrandId = command.BrandId
            });

        var expectedCreateRequest = new WithdrawFormCreateInfo
        {
            Amount = command.Amount,
            BankInfoId = command.BankInfoId,
            BrandId = command.BrandId,
            Note = command.Note,
            UserId = command.PlayerCode,
            UserName = "user1",
            UserType = UserType.Player
        };

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient.Received(1)
            .CreateWithdrawFormAsync(Arg.Is<WithdrawFormCreateInfo>(
                info => expectedCreateRequest.ToExpectedObject().Matches(info)));
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(Arg.Any<Guid>(), Arg.Is(PaymentFlowState.WaitApprove));
    }
}
