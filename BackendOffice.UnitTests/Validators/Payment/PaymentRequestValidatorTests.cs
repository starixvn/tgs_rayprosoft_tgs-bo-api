using FluentValidation.TestHelper;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Apis.BackendOffice.Controllers.Messages.Validators.BrandPayment;
using TGS.Apis.BackendOffice.Resources;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.Payment;

[TestFixture]
public class BrandPaymentRequestValidatorTests
{
    private BrandPaymentRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new BrandPaymentRequestValidator();
    }

    [Test]
    public void Should_have_error_when_channel_name_is_null()
    {
        var model = new BrandPaymentRequest { ChannelName = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.ChannelName)
            .WithErrorMessage(BrandPayment.ChannelNameRequired);
    }

    [Test]
    public void Should_have_error_when_payment_vendor_id_is_null()
    {
        var model = new BrandPaymentRequest { PaymentMethodChannelId = Guid.Empty };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.PaymentMethodChannelId)
            .WithErrorMessage(BrandPayment.PaymentRequired);
    }

    [Test]
    public void Should_have_error_when_transfer_limit_to_is_null()
    {
        var model = new BrandPaymentRequest { TransferLimitTo = 0 };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.TransferLimitTo)
            .WithErrorMessage(BrandPayment.TransferLimitGreaterZero);
    }

    [Test]
    public void Should_not_have_error_when_channel_name_is_specified()
    {
        var model = new BrandPaymentRequest { ChannelName = "ChannelName" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.ChannelName);
    }

    [Test]
    public void Should_not_have_error_when_deposited_amount_to_is_specified()
    {
        var model = new BrandPaymentRequest { DepositedAmountMax = 1 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.DepositedAmountMax);
    }

    [Test]
    public void Should_not_have_error_when_payment_vendor_id_is_specified()
    {
        var model = new BrandPaymentRequest { PaymentMethodChannelId = Guid.NewGuid() };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.PaymentMethodChannelId);
    }

    [Test]
    public void Should_not_have_error_when_transfer_limit_to_is_specified()
    {
        var model = new BrandPaymentRequest { TransferLimitTo = 1 };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.TransferLimitTo);
    }
}
