using FluentValidation.TestHelper;
using TGS.Services.BackOffice.Application.Models.Brand;
using TGS.Services.BackOffice.Validation.Brand;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.Brand;

[TestFixture]
public class UpdateBrandRequestValidatorTests
{
    private UpdateBrandRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateBrandRequestValidator();
    }

    [Test]
    public void Should_have_error_when__brand_cultures_is_null()
    {
        var model = new UpdateBrandRequest { Languages = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Languages)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Brand.CultureRequired);
    }

    [Test]
    public void Should_have_error_when__brand_default_bet_limit_is_not_in_currencies()
    {
        var model = new UpdateBrandRequest
        {
            CurrencyCodes = ["CNY", "RMB"],
            BetLimits = [new BrandBetLimitModel { Id = 1, CurrencyCode = "CNY", IsDefault = false }]
        };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.CurrencyCodes)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Brand.BetLimitDefaultRequired);
    }

    [Test]
    public void Should_have_error_when__currency_codes_is_null()
    {
        var model = new UpdateBrandRequest { CurrencyCodes = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.CurrencyCodes)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Brand.CurrencyRequired);
    }

    [Test]
    public void Should_have_error_when__game_providers_is_null()
    {
        var model = new UpdateBrandRequest { GameProviders = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.GameProviders)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Brand.GameProviderRequired);
    }

    [Test]
    public void Should_have_error_when_code_is_null()
    {
        var model = new UpdateBrandRequest { Code = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Code)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Brand.CodeRequired);
    }

    [Test]
    public void Should_have_error_when_name_is_null()
    {
        var model = new UpdateBrandRequest { Name = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Brand.NameRequired);
    }
}
