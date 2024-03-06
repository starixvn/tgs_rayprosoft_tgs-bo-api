using AutoFixture;
using FluentValidation.TestHelper;
using TGS.Services.BackOffice.Application.Models.BrandGameProviderSchedule;
using TGS.Services.BackOffice.Validation.BrandGameProviderSchedule;

namespace TGS.Tests.BackendOffice.UnitTests.Validators.BrandGameProviderSchedule;

[TestFixture]
public class UpdateDefaultTimesRequestValidatorTests
{
    private UpdateDefaultTimesRequestValidator _validator = default!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateDefaultTimesRequestValidator();
    }

    [Test]
    public void Should_have_error_when_from_date_greater_than_to_date()
    {
        var fixture = new Fixture();
        var time = fixture.Build<DefaultTimeModel>()
            .With(x => x.From, DateTimeOffset.UtcNow.AddDays(1))
            .With(x => x.To, DateTimeOffset.UtcNow).Create();
        var model = fixture.Build<UpdateDefaultTimesRequest>().With(x => x.DefaultTimes, [time]).Create();

        var result = _validator.TestValidate(model);
        _ = result.ShouldHaveValidationErrorFor("DefaultTimes[0].From")
            .WithErrorMessage(Services.BackOffice.Application.Resources.BrandGameProviderSchedule
                .ToDateGreaterThanFrom);
    }
}
