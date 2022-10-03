using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.Payment;
using TGS.Services.BackOffice.Application.Queries.BrandPayment;
using Tgs.ServiceUtility;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : BaseContextController
{
    /// <summary>
    /// Creates the payment.
    /// </summary>
    /// <param name="createService">The create service.</param>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [HttpPut]
    public ValueTask CreatePaymentAsync(
        [FromServices] ICommandService<CreatePaymentCommand> createService,
        [FromBody] CreatePaymentRequest request)
    {
        var command = new CreatePaymentCommand(
            request.Code,
            request.Description,
            request.ExtendedData);

        return createService.ExecuteAsync(command);
    }

    [HttpGet]
    [Description("Get Payments")]
    public GenericResponse<IEnumerable<PaymentModel>> GetAllPayments(
        [FromServices] IQueryService<AllPaymentQuery, PaymentInfo> queryService)
    {
        return new GenericResponse<IEnumerable<PaymentModel>>
        {
            Valid = true,
            Data = queryService.QueryAsync(new AllPaymentQuery())
                .Select(p => new PaymentModel
                {
                    Id = p.Id,
                    Name = p.Code,
                    Code = p.Code
                })
                .ToEnumerable()
        };
    }
}
