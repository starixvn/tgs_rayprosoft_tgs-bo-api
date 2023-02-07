using FluentValidation.TestHelper;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Validation.BrandGameProviderSchedule;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.BrandGameProviderSchedule;

[TestFixture]
public class BrandGameProviderRequestValidatorTests
{
    private ScheduleRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new ScheduleRequestValidator();
    }

    [Test]
    public void Should_have_error_when_day_of_week_is_not_in_enums()
    {
        var model = new BrandGameProviderRequest
        {
            DayOfWeek = (DayOfWeek)8
        };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.DayOfWeek)
            .WithErrorMessage(
                Services.BackOffice.Application.Resources.BrandGameProviderSchedule.DayOfWeekIncorrect);
    }

    [Test]
    public void Should_have_error_when_from_date_greater_than_to_date()
    {
        var model = new BrandGameProviderRequest
        { From = DateTimeOffset.UtcNow.AddDays(1), To = DateTimeOffset.UtcNow };
        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor(request => request.From)
            .WithErrorMessage(Services.BackOffice.Application.Resources.BrandGameProviderSchedule
                .ToDateGreaterThanFrom);
    }
}
