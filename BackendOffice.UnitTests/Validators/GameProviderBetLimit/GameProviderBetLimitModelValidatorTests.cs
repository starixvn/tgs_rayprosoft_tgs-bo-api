using FluentValidation.TestHelper;
using TGS.Services.BackOffice.Application.Models.GameProviderBetLimit;
using TGS.Services.BackOffice.Validation.GameProviderBetLimit;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.GameProviderBetLimit;

[TestFixture]
public class GameProviderBetLimitModelValidatorTests
{
    private GameProviderBetLimitModelValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new GameProviderBetLimitModelValidator();
    }

    [Test]
    public void Should_have_error_when_code_is_null()
    {
        var model = new GameProviderBetLimitModel
        {
            CurrencyCode = null
        };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.CurrencyCode)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProviderBetLimit.CurrencyRequired);
    }

    [Test]
    public void Should_have_error_when_max_bet_is_not_greater_min_bet()
    {
        var model = new GameProviderBetLimitModel
        {
            MinBet = 50,
            MaxBet = 30
        };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.MaxBet)
            .WithErrorMessage(
                Services.BackOffice.Application.Resources.GameProviderBetLimit.MaxbetGreaterThanMinbet);
    }

    [Test]
    public void Should_have_error_when_min_bet_is_not_greater_zero()
    {
        var model = new GameProviderBetLimitModel
        {
            MinBet = -1
        };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.MinBet)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProviderBetLimit.MinbetGreaterThanZero);
    }
}
