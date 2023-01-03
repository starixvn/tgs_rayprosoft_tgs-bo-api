using FluentValidation.TestHelper;
using NUnit.Framework;
using TGS.Apis.BackendOffice.Controllers.Messages.Users;
using TGS.Apis.BackendOffice.Controllers.Messages.Validators.User;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.User;

[TestFixture]
public class UpdateUserRequestValidatorTests
{
    private UpdateUserRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateUserRequestValidator();
    }

    [Test]
    public void Should_have_error_name_is_null()
    {
        var model = new UpdateUserRequest { Name = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Apis.BackendOffice.Resources.User.NameRequired);
    }

    [Test]
    public void Should_not_have_error_when_channel_name_is_specified()
    {
        var model = new UpdateUserRequest { Name = "Name" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.Name);
    }
}
