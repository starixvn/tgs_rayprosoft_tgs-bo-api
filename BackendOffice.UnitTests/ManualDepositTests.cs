using System;
using System.Threading;
using System.Threading.Tasks;
using ExpectedObjects;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.BrandCertificate.Clients;
using Tgs.Payment.Clients;
using Tgs.Payment.ManualDeposit;
using Tgs.Promotion.Clients;
using TGS.Payment.ManualDeposit;
using TGS.Services.BackOffice.ApplicationServices;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class ManualDepositTests
{
    [Test]
    public async Task Create_Bank_Deposit_Form_Should_Success()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();
        var moqPaymentClient = new Mock<IPaymentClient>();
        var serializer = new ManualDepositSerializer();
        var moqPromotionClient = new Mock<IPromotionClient>();

        var sut = new CreateDepositFormCommandService(
            fakeBrandServiceClient,
            moqPaymentServiceClient.Object,
            moqPaymentClient.Object,
            moqPromotionClient.Object);

        var command = new CreateDepositFormCommand(
            "ThisIsPlayerCode",
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000M,
            "tgs.payment.manual",
            null,
            _ => serializer.Serialize(
                new ManualDepositBankApplyInfo()
                {
                    SendBankAccount = "ThisIsBankAccount",
                    ReceiveBankAccount = "ThisIsReceiveBankAccount",
                    ReceiveAccountName = "ReceiveAccountName",
                    BankType = "Bank"
                }));
        var paymentId = Guid.NewGuid();

        moqPaymentClient
            .Setup(fake => fake.DepositAsync(
                It.IsAny<string>(),
                It.Is<decimal>(x => x == command.Amount),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult(command.CreatePaymentSettings(Guid.NewGuid())!));

        moqPaymentServiceClient
            .Setup(fake => fake.GetBrandPaymentByIdAsync(
                It.Is<Guid>(x => x == command.BrandPaymentId),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<BrandPaymentInfo?>(new BrandPaymentInfo
            {
                Id = command.BrandPaymentId,
                PaymentId = paymentId
            }));

        moqPaymentServiceClient
            .Setup(fake => fake.GetPaymentByIdAsync(
                It.Is<Guid>(x => x == paymentId),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<PaymentInfo?>(new PaymentInfo
            {
                Id = paymentId,
                Code = "Test"
            }));

        var expected = new DepositFormCreateInfo
        {
            PlayerCode = command.PlayerCode,
            BrandId = command.BrandId,
            BrandPaymentId = command.BrandPaymentId,
            Amount = command.Amount,
            PromotionId = command.PromotionId,
            ApplyInfo = serializer.Serialize(
                new ManualDepositBankApplyInfo()
                {
                    BankType = "Bank",
                    SendBankAccount = "ThisIsBankAccount",
                    ReceiveBankAccount = "ThisIsReceiveBankAccount",
                    ReceiveAccountName = "ReceiveAccountName"
                })
        }.ToExpectedObject();

        await sut.GetAsync(command);

        moqPaymentServiceClient.Verify(
            fake => fake.CreateDepositFormAsync(
                It.IsAny<Guid>(),
                It.Is<DepositFormCreateInfo>(form => expected.Matches(form)),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Test]
    public void Create_DepositForm_Can_Not_Lock_Payment_Should_Fail()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();

        var sut = new CreateDepositFormCommandService(
            fakeBrandServiceClient,
            fakePaymentServiceClient,
            fakePaymentClient,
            fakePromotionClient);

        var command = new CreateDepositFormCommand(
            "ThisIsPlayerCode",
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000M,
            "tgs.payment.leepay",
            null,
            _ => "{}");
        var paymentId = Guid.NewGuid();

        fakePaymentServiceClient.GetBrandPaymentByIdAsync(
            Arg.Is(command.BrandPaymentId))
            .Returns(new BrandPaymentInfo
            {
                Id = command.BrandPaymentId,
                PaymentId = paymentId
            });

        fakePaymentServiceClient.GetPaymentByIdAsync(
            Arg.Is(paymentId))
            .Returns(new PaymentInfo
            {
                Id = paymentId,
                Code = "Test"
            });

        fakeBrandServiceClient.GetBrandPaymentSettingsAsync(
            Arg.Is(command.BrandId))
            .Returns(new BrandPaymentSettings(true));

        Assert.ThrowsAsync<Exception>(async () => await sut.GetAsync(command));
    }

    [Test]
    public void Create_DepositForm_Not_Found_BrandPayment_Should_Fail()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var fakePromotionClient = Substitute.For<IPromotionClient>();

        var sut = new CreateDepositFormCommandService(
            fakeBrandServiceClient,
            fakePaymentServiceClient,
            fakePaymentClient,
            fakePromotionClient);

        var command = new CreateDepositFormCommand(
            "ThisIsPlayerCode",
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000M,
            "tgs.payment.leepay",
            Guid.NewGuid(),
            _ => "{}");

        Assert.ThrowsAsync<Exception>(async () => await sut.GetAsync(command));
    }

    [Test]
    public void Daily_Second_Deposit_With_First_Deposit_Promotion_Should_Fail()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var moqPaymentServiceClient = new Mock<IPaymentServiceClient>();
        var moqPaymentClient = new Mock<IPaymentClient>();
        var moqPromotionClient = new Mock<IPromotionClient>();

        var sut = new CreateDepositFormCommandService(
            fakeBrandServiceClient,
            moqPaymentServiceClient.Object,
            moqPaymentClient.Object,
            moqPromotionClient.Object);

        var command = new CreateDepositFormCommand(
            "ThisIsPlayerCode",
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000M,
            "tgs.payment.leepay",
            Guid.NewGuid(),
            _ => "{}");
        var paymentId = Guid.NewGuid();

        moqPaymentServiceClient
            .Setup(fake => fake.GetBrandPaymentByIdAsync(
                It.Is<Guid>(x => x == command.BrandPaymentId),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<BrandPaymentInfo?>(new BrandPaymentInfo
            {
                Id = command.BrandPaymentId,
                PaymentId = paymentId
            }));

        moqPaymentServiceClient
            .Setup(fake => fake.GetPaymentByIdAsync(
                It.Is<Guid>(x => x == paymentId),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<PaymentInfo?>(new PaymentInfo
            {
                Id = paymentId,
                Code = "Test"
            }));

        Assert.ThrowsAsync<Exception>(async () => await sut.GetAsync(command));
    }
}
