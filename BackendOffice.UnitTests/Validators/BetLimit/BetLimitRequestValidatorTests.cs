using FluentValidation.TestHelper;
using NUnit.Framework;
using TGS.Services.BackOffice.Application.Models.BetLimit;
using TGS.Services.BackOffice.Validation.BetLimit;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.BetLimit;

[TestFixture]
public class BetLimitRequestValidatorTests
{
    [SetUp]
    public void Setup()
    {
        _validator = new BetLimitRequestValidator();
    }

    private BetLimitRequestValidator _validator = default!;

    [Test]
    public void Should_have_error_when_currency_code_is_null()
    {
        var model = new BetLimitRequest {CurrencyCode = null};
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(request => request.CurrencyCode)
            .WithErrorMessage(Services.BackOffice.Application.Resources.BetLimit.CodeRequired);
    }

    [Test]
    public void Should_have_error_when_min_bet_is_not_greater_zero()
    {
        var model = new BetLimitRequest
        {
            MinBet = -1
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(request => request.MinBet)
            .WithErrorMessage(Services.BackOffice.Application.Resources.BetLimit.MinbetGreaterThanZero);
    }

    [Test]
    public void Should_have_error_when_max_bet_is_not_greater_min_bet()
    {
        var model = new BetLimitRequest
        {
            MinBet = 50,
            MaxBet = 30
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(request => request.MaxBet)
            .WithErrorMessage(
                Services.BackOffice.Application.Resources.BetLimit.MaxbetGreaterThanMinbet);
    }
}