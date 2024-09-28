using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.PaymentChannel;
using TGS.Services.BackOffice.Application.Queries.PaymentChannel;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

[Route("api/[controller]")]
[ApiController]
public class PaymentChannelController : BaseContextController
{
    /// <summary>
    /// Creates the payment channel asynchronous.
    /// </summary>
    /// <param name="createService">The create service.</param>
    /// <param name="code">The code.</param>
    /// <returns></returns>
    [HttpPut]
    public ValueTask CreatePaymentChannelAsync(
        [FromServices] ICommandService<CreatePaymentChannelCommand> createService,
        [FromBody] string code)
    {
        var command = new CreatePaymentChannelCommand(code);

        return createService.ExecuteAsync(command);
    }

    [HttpGet]
    [BOAuthorize(["System.BrandPayments", "SystemAdmin.BrandPayments", "FundManagement", "PlayerManagement.PlayerList", "Promotion.PromotionList"])]
    [Description("Get PaymentChannels")]
    public GenericResponse<IEnumerable<PaymentChannelModel>> GetAllPaymentChannels(
        [FromServices] IQueryService<PaymentChannelQuery, PaymentChannelInfo> queryService)
    {
        return new GenericResponse<IEnumerable<PaymentChannelModel>>
        {
            Valid = true,
            Data = queryService.QueryAsync(new PaymentChannelQuery())
                .Select(pc => new PaymentChannelModel
                {
                    Id = pc.Id,
                    Code = pc.Name
                })
                .ToEnumerable()
        };
    }
}