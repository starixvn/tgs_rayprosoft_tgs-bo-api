using FluentValidation.TestHelper;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Apis.BackendOffice.Controllers.Messages.Validators.User;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.User;

[TestFixture]
public class CreateUserRequestValidatorTests
{
    private CreateUserRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateUserRequestValidator();
    }

    [Test]
    public void Should_have_error_name_is_null()
    {
        var model = new CreateUserRequest { Name = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.NameRequired);
    }

    [Test]
    public void Should_have_error_password_is_not_equal_to_confirmed_password()
    {
        var model = new CreateUserRequest { Password = "Password", ConfirmPassword = "ConfirmedPassword" };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordNotMatched);
    }

    [Test]
    public void Should_have_error_password_is_null()
    {
        var model = new CreateUserRequest { Password = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordRequired);
    }

    [Test]
    public void Should_have_error_password_length_is_not_greater_than_or_equal_to_eight()
    {
        var model = new CreateUserRequest { Password = "Pswd", ConfirmPassword = "Pswd" };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordLengthAtLeast);
    }

    [Test]
    public void Should_not_have_error_when_name_is_specified()
    {
        var model = new CreateUserRequest { Name = "Name" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.Name);
    }

    [Test]
    public void Should_not_have_error_when_password_have_not_number()
    {
        var model = new CreateUserRequest { Password = "P@ssword", ConfirmPassword = "P@ssword" };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordNeedNumber);
    }

    [Test]
    public void Should_not_have_error_when_password_have_not_special_character()
    {
        var model = new CreateUserRequest { Password = "11111111", ConfirmPassword = "11111111" };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Password)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.PasswordNeedCharacter);
    }

    [Test]
    public void Should_not_have_error_when_password_is_equal_to_confirmed_password()
    {
        var model = new CreateUserRequest { Password = "P@ssw0rd", ConfirmPassword = "P@ssw0rd" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.Password);
    }
}
