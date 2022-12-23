using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TGS.Apis.BackendOffice.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    async Task IAsyncActionFilter.OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errorsInModelState = context.ModelState
                .Where(p => p.Value.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => x.ErrorMessage)).ToArray();

            var response = new ErrorResponse();

            foreach (var error in errorsInModelState)
            {
                foreach (var aubError in error.Value)
                {
                    var errorModel = new ErrorModel
                    {
                        FieldName = error.Key,
                        Message = aubError
                    };
                    response.Errors.Add(errorModel);
                }
            }

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        _ = await next();
    }
}
