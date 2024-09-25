using ExpectedObjects;
using Tgs.BrandCertificate.Clients;
using Tgs.Payment.Clients;
using Tgs.Payment.ManualDeposit;
using TGS.Payment.ManualDeposit;
using Tgs.Promotion.Clients;
using TGS.Services.BackOffice.ApplicationServices.Payment;

namespace TGS.Tests.BackendOffice.UnitTests;

[TestFixture]
public class ManualDepositTests
{
    [Test]
    public async Task Create_Bank_Deposit_Form_Should_Success()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var moqPaymentClient = Substitute.For<IPaymentClient>();
        var serializer = new ManualDepositSerializer();
        var moqPromotionClient = Substitute.For<IPromotionClient>();

        var sut = new CreateDepositFormCommandService(
            fakeBrandServiceClient,
            moqPaymentServiceClient,
            moqPaymentClient,
            moqPromotionClient);

        var command = new CreateDepositFormCommand(
            "ThisIsPlayerCode",
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000M,
            "tgs.payment.manual",
            null,
            _ => serializer.Serialize(
                new ManualDepositBankApplyInfo
                {
                    SendBankAccount = "ThisIsBankAccount",
                    ReceiveBankAccount = "ThisIsReceiveBankAccount",
                    ReceiveAccountName = "ReceiveAccountName",
                    BankType = "Bank"
                }));
        var paymentId = Guid.NewGuid();

        _ = moqPaymentClient
            .DepositAsync(
                Arg.Any<string>(),
                Arg.Is<decimal>(x => x == command.Amount),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult(command.CreatePaymentSettings(Guid.NewGuid())!));

        _ = moqPaymentServiceClient
            .GetBrandPaymentByIdAsync(
                Arg.Is<Guid>(x => x == command.BrandPaymentId),
                Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<BrandPaymentInfo?>(new()
            {
                Id = command.BrandPaymentId,
                PaymentId = paymentId
            }));

        _ = moqPaymentServiceClient
            .GetPaymentByIdAsync(
                Arg.Is<Guid>(x => x == paymentId),
                Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<PaymentInfo?>(new()
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
                new ManualDepositBankApplyInfo
                {
                    BankType = "Bank",
                    SendBankAccount = "ThisIsBankAccount",
                    ReceiveBankAccount = "ThisIsReceiveBankAccount",
                    ReceiveAccountName = "ReceiveAccountName"
                })
        }.ToExpectedObject();

        _ = await sut.GetAsync(command);

        _ = moqPaymentServiceClient.Received(1).CreateDepositFormAsync(
            Arg.Any<Guid>(),
            Arg.Is<DepositFormCreateInfo>(form => expected.Matches(form)),
            Arg.Any<CancellationToken>());
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

        _ = fakePaymentServiceClient.GetBrandPaymentByIdAsync(
                Arg.Is(command.BrandPaymentId))
            .Returns(new BrandPaymentInfo
            {
                Id = command.BrandPaymentId,
                PaymentId = paymentId
            });

        _ = fakePaymentServiceClient.GetPaymentByIdAsync(
                Arg.Is(paymentId))
            .Returns(new PaymentInfo
            {
                Id = paymentId,
                Code = "Test"
            });

        _ = fakeBrandServiceClient.GetBrandPaymentSettingsAsync(
                Arg.Is(command.BrandId))
            .Returns(new BrandPaymentSettings(true, false, 0M));

        _ = Assert.ThrowsAsync<Exception>(async () => await sut.GetAsync(command));
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

        _ = Assert.ThrowsAsync<Exception>(async () => await sut.GetAsync(command));
    }

    [Test]
    public void Daily_Second_Deposit_With_First_Deposit_Promotion_Should_Fail()
    {
        var fakeBrandServiceClient = Substitute.For<IBrandServiceClient>();
        var moqPaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var moqPaymentClient = Substitute.For<IPaymentClient>();
        var moqPromotionClient = Substitute.For<IPromotionClient>();

        var sut = new CreateDepositFormCommandService(
            fakeBrandServiceClient,
            moqPaymentServiceClient,
            moqPaymentClient,
            moqPromotionClient);

        var command = new CreateDepositFormCommand(
            "ThisIsPlayerCode",
            Guid.NewGuid(),
            Guid.NewGuid(),
            1000M,
            "tgs.payment.leepay",
            Guid.NewGuid(),
            _ => "{}");
        var paymentId = Guid.NewGuid();

        _ = moqPaymentServiceClient
            .GetBrandPaymentByIdAsync(
                Arg.Is<Guid>(x => x == command.BrandPaymentId),
                Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<BrandPaymentInfo?>(new()
            {
                Id = command.BrandPaymentId,
                PaymentId = paymentId
            }));

        _ = moqPaymentServiceClient
            .GetPaymentByIdAsync(
                Arg.Is<Guid>(x => x == paymentId),
                Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<PaymentInfo?>(new()
            {
                Id = paymentId,
                Code = "Test"
            }));

        _ = Assert.ThrowsAsync<Exception>(async () => await sut.GetAsync(command));
    }
}