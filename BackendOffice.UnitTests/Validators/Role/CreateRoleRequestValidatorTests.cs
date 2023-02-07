using AutoFixture;
using FluentValidation.TestHelper;
using TGS.Apis.BackendOffice.Controllers.Messages.Roles;
using TGS.Apis.BackendOffice.Controllers.Messages.Validators.Role;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.Role;

[TestFixture]
public class CreateRoleRequestValidatorTests
{
    private CreateRoleRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateRoleRequestValidator();
    }

    [Test]
    public void Should_have_error_name_is_null()
    {
        var model = new CreateRoleRequest { Name = null };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Apis.BackendOffice.Resources.Role.NameRequired);
    }

    [Test]
    public void Should_have_error_name_length_is_greater_then_fifty()
    {
        var fixture = new Fixture();
        var name = string.Join(string.Empty, fixture.CreateMany<string>(2));
        var model = new CreateRoleRequest { Name = name };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.Name)
            .WithErrorMessage(Apis.BackendOffice.Resources.Role.NameLengthLimit);
    }

    [Test]
    public void Should_not_have_error_when_name_is_specified()
    {
        var model = new CreateRoleRequest { Name = "RealName" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.Name);
    }

    [Test]
    public void Should_not_have_error_when_name_length_is_not_greater_then_fifty()
    {
        var model = new CreateRoleRequest { Name = "Name" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(request => request.Name);
    }
}
