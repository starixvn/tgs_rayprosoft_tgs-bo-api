using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TGS.Apis.BackendOffice.Controllers.Messages.Deposit;

namespace TGS.Apis.BackendOffice.Services;

public class CommaSeparatedStringPaymentStatusBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var modelName = bindingContext.ModelName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        // Check if the argument value is null or empty
        if (string.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var model = value.Split(",")
            .Select(v => (FlowState)Enum.Parse(typeof(FlowState), v.Trim()))
            .ToArray();

        bindingContext.Result = ModelBindingResult.Success(model);

        return Task.CompletedTask;
    }
}
