using FluentValidation;
using TGS.Apis.BackendOffice.ViewModels;

namespace TGS.Apis.BackendOffice.Validators;

public class AdjustPlayerBucketViewModelValidator : AbstractValidator<AdjustPlayerBucketViewModel>
{
    public AdjustPlayerBucketViewModelValidator()
    {
        _ = RuleFor(model => model.Amount)
            .GreaterThan(0);
    }
}
