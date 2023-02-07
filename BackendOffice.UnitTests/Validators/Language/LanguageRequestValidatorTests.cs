using FluentValidation.TestHelper;
using TGS.Services.BackOffice.Application.Models.Language;
using TGS.Services.BackOffice.Validation.Language;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.Language;

[TestFixture]
public class LanguageRequestValidatorTests
{
    private LanguageRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new LanguageRequestValidator();
    }

    [Test]
    public void Should_have_error_when_code_is_null()
    {
        var model = new LanguageRequest { CultureCode = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.CultureCode)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Language.CultureCodeRequired);
    }

    [Test]
    public void Should_have_error_when_name_is_null()
    {
        var model = new LanguageRequest { LanguageCode = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.LanguageCode)
            .WithErrorMessage(Services.BackOffice.Application.Resources.Language.LanguageCodeRequired);
    }
}
