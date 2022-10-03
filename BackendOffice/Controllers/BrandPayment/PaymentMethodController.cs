using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;
using TGS.Services.BackOffice.Application.Queries.PaymentMethod;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

[Route("api/[controller]")]
[ApiController]
public class PaymentMethodController : BaseContextController
{
    /// <summary>
    /// Creates the payment method asynchronous.
    /// </summary>
    /// <param name="createService">The create service.</param>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [HttpPut]
    public ValueTask CreatePaymentMethodAsync(
        [FromServices] ICommandService<CreatePaymentMethodCommand> createService,
        [FromBody] CreatePaymentMethodRequest request)
    {
        var command = new CreatePaymentMethodCommand(request.Code, request.BankType);

        return createService.ExecuteAsync(command);
    }

    [HttpGet]
    [Description("Get PaymentMethods")]
    public GenericResponse<IEnumerable<PaymentMethodModel>> GetAllPaymentMethods(
        [FromServices] IQueryService<PaymentMethodQuery, PaymentMethodInfo> queryService)
    {
        return new GenericResponse<IEnumerable<PaymentMethodModel>>
        {
            Valid = true,
            Data = queryService.QueryAsync(new PaymentMethodQuery())
                .Select(pm => new PaymentMethodModel
                {
                    Id = pm.Id,
                    Code = pm.Name,
                    BankType = pm.BankType
                })
                .ToEnumerable()
        };
    }
}
