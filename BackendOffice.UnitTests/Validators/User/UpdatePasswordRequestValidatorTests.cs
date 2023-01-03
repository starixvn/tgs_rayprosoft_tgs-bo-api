using FluentValidation.TestHelper;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Apis.BackendOffice.Controllers.Messages.Validators.User;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.User;

[TestFixture]
public class UpdatePasswordRequestValidatorTests
{
    private UpdatePasswordRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdatePasswordRequestValidator();
    }

    [Test]
    public void Should_have_error_name_is_null()
    {
        var model = new UpdatePasswordRequest { Password = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordRequired);
    }

    [Test]
    public void Should_have_error_password_is_not_equal_to_confirmed_password()
    {
        var model = new UpdatePasswordRequest { Password = "Password", ConfirmPassword = "ConfirmPassword" };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordNotMatched);
    }

    [Test]
    public void Should_not_have_error_when_password_is_specified()
    {
        var model = new UpdatePasswordRequest { Password = "P@ssw0rd", ConfirmPassword = "P@ssw0rd" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.Password);
    }
}
