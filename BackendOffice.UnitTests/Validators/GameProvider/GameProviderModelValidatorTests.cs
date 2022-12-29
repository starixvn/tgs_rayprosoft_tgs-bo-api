using FluentValidation.TestHelper;
using NUnit.Framework;
using TGS.Core.Models.Enums;
using TGS.Services.BackOffice.Application.Models.GameProvider;
using TGS.Services.BackOffice.Validation.GameProvider;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.GameProvider;

[TestFixture]
public class GameProviderModelValidatorTests
{
    private GameProviderModelValidator _validator = default!;

    [SetUp]
    public void Setup() => _validator = new();

    [Test]
    public void Should_have_error_when_code_is_null()
    {
        var model = new GameProviderModel { Code = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Code)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProvider.CodeRequired);
    }

    [Test]
    public void Should_have_error_when_currency_is_null()
    {
        var model = new GameProviderModel { Currencies = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Currencies)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProvider.CurrencyRequired);
    }

    [Test]
    public void Should_have_error_when_language_is_null()
    {
        var model = new GameProviderModel { Languages = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Languages)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProvider.LanguageRequired);
    }

    [Test]
    public void Should_have_error_when_name_is_null()
    {
        var model = new GameProviderModel { Name = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProvider.NameRequired);
    }

    [Test]
    public void Should_have_error_when_status_is_not_in_enums()
    {
        var model = new GameProviderModel { Status = (Status)5 };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Status)
            .WithErrorMessage(Services.BackOffice.Application.Resources.GameProvider.StatusRequired);
    }
}
