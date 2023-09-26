using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TGS.Apis.BackendOffice.ViewModels;

public class DateTimeOffsetToDateOnlyModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var bindValue = bindingContext.ValueProvider
            .GetValue(bindingContext.BinderModelName)
            .Values
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(bindValue) && DateTimeOffset.TryParse(bindValue, out var dateTime))
            bindingContext.Result = ModelBindingResult.Success(
                DateOnly.FromDateTime(dateTime.ToOffset(TimeSpan.FromHours(8D)).DateTime));

        return Task.CompletedTask;
    }
}
