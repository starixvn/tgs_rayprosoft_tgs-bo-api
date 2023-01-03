using FluentValidation.TestHelper;
using NUnit.Framework;
using TGS.Services.BackOffice.Application.Models.Currency;
using TGS.Services.BackOffice.Validation.Currency;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.Currency;

[TestFixture]
public class CurrencyRequestValidatorTests
{
    private CurrencyRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new CurrencyRequestValidator();
    }

    [Test]
    public void Should_have_error_when_code_is_null()
    {
        var model = new CurrencyRequest { Code = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Code)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Currency.CodeRequired);
    }

    [Test]
    public void Should_have_error_when_name_is_null()
    {
        var model = new CurrencyRequest { Name = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Currency.NameRequired);
    }
}
