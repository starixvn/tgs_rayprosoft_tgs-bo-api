using System;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.Payment.Clients;
using TGS.Services.BackOffice.Application.Commands.PlayerBank;

namespace TGS.Tests.BackendOffice.UnitTests;

internal class PlayerBankInfoTests
{
    [Test]
    public async Task Create_Player_BankInfo_Should_Success()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new CreatePlayerBankInfoCommandService(fakePaymentServiceClient);

        var brandId = Guid.NewGuid();
        var playerCode = "ThisIsPlayerCode";
        var paymentChannelId = Guid.NewGuid();
        var bankType = "Bank";
        var detail = new
        {
            bankAccount = "1234567890"
        };

        var command = new CreatePlayerBankInfoCommand(
            playerCode,
            brandId,
            paymentChannelId,
            bankType,
            JsonSerializer.SerializeToDocument(detail),
            "bo_admin");

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient
            .Received(1)
            .CreatePlayerBankInfoAsync(
                Arg.Is(command.BrandId),
                Arg.Is(command.PlayerCode),
                Arg.Is<BankCreateInfo>(x => x.PaymentChannelId == command.PaymentChannelId),
                Arg.Is(command.ModifyUser));
    }

    [Test]
    public async Task Delete_Player_BankInfo_Should_Success()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new DeletePlayerBankInfoCommandService(fakePaymentServiceClient);

        var playerCode = "ThisIsPlayerCode";
        var bankInfoId = Guid.NewGuid();

        var command = new DeletePlayerBankInfoCommand(playerCode, bankInfoId);

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient
            .Received(1)
            .DeletePlayerBankInfoAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.BankInfoId));
    }

    [Test]
    public async Task Set_Default_Player_BankInfo_Should_Success()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new SetDefaultPlayerBankInfoCommandService(fakePaymentServiceClient);

        var playerCode = "ThisIsPlayerCode";
        var bankInfoId = Guid.NewGuid();
        var modifyUser = "bo_admin";

        var command = new SetDefaultPlayerBankInfoCommand(playerCode, bankInfoId, modifyUser);

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient
            .Received(1)
            .SetDefaultPlayerBankInfoAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.BankInfoId),
                Arg.Is(command.ModifyUser));
    }

    [Test]
    public async Task Update_Player_BankInfo_Should_Success()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();

        var sut = new UpdatePlayerBankInfoCommandService(fakePaymentServiceClient);

        var brandId = Guid.NewGuid();
        var bankInfoId = Guid.NewGuid();
        var playerCode = "ThisIsPlayerCode";
        var paymentChannelId = Guid.NewGuid();
        var bankType = "Bank";
        var detail = new
        {
            bankAccount = "1234567890"
        };

        var command = new UpdatePlayerBankInfoCommand(
            playerCode,
            bankInfoId,
            brandId,
            paymentChannelId,
            bankType,
            JsonSerializer.SerializeToDocument(detail),
            "bo_admin");

        await sut.ExecuteAsync(command);

        _ = fakePaymentServiceClient
            .Received(1)
            .DeletePlayerBankInfoAsync(
                Arg.Is(command.PlayerCode),
                Arg.Is(command.BankInfoId));

        _ = fakePaymentServiceClient
            .Received(1)
            .CreatePlayerBankInfoAsync(
                Arg.Is(command.BrandId),
                Arg.Is(command.PlayerCode),
                Arg.Is<BankCreateInfo>(x => x.PaymentChannelId == command.PaymentChannelId),
                Arg.Is(command.ModifyUser));
    }
}
