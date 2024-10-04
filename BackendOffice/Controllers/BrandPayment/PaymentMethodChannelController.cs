using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;
using TGS.Apis.BackendOffice.Attributes;
using TGS.Apis.BackendOffice.Controllers.Messages;
using TGS.Apis.BackendOffice.Controllers.Messages.BrandPayment;
using TGS.Apis.BackendOffice.Controllers.Messages.PaymentChannel;
using TGS.Apis.BackendOffice.Controllers.Messages.PaymentMethod;
using TGS.Services.BackOffice.Application.Commands.PaymentMethodChannel;
using TGS.Services.BackOffice.Application.Queries.BrandPayment;
using TGS.Services.BackOffice.Application.Queries.PaymentChannel;
using TGS.Services.BackOffice.Application.Queries.PaymentMethod;

namespace TGS.Apis.BackendOffice.Controllers.BrandPayment;

[Route("api/pmc")]
[ApiController]
public class PaymentMethodChannelController : ControllerBase
{
    [HttpPost]
    [BOAuthorize(["SystemAdmin.BrandPayments.Update"])]
    public async Task CreatePaymentMethodChannelAsync(
        [FromServices] ICommandService<CreatePaymentMethodChannelCommand> createService,
        [FromBody] CreatePaymentMethodChannelRequest request)
    {
        var command = new CreatePaymentMethodChannelCommand(
            request.PaymentId,
            request.MethodId,
            request.ChannelId);

        await createService.ExecuteAsync(command).ConfigureAwait(false);
    }

    [HttpDelete("{id}")]
    [BOAuthorize(["SystemAdmin.BrandPayments.Update"])]
    public async Task DeletePaymentMethodChannelAsync(
        [FromServices] ICommandService<DeletePaymentMethodChannelCommand> deleteService,
        Guid id)
    {
        var command = new DeletePaymentMethodChannelCommand(id);

        await deleteService.ExecuteAsync(command).ConfigureAwait(false);
    }

    [HttpGet]
    [HttpGet("~/api/brandpayment/pmc")]
    [BOAuthorize(["System.BrandPayments.Update", "SystemAdmin.BrandPayments", "Promotion.PromotionList"])]
    [Description("Get All Brand PaymentMethodChannels")]
    public async Task<GenericResponse<AllPaymentMethodChannelsResponse>> GetAllPaymentMethodChannelsAsync(
        [FromServices] IQueryService<AllPaymentMethodChannelQuery, PaymentMethodChannelInfo> queryService,
        [FromServices] IQueryService<PaymentMethodQuery, PaymentMethodInfo> paymentMethodQueryService,
        [FromServices] IQueryService<PaymentChannelQuery, PaymentChannelInfo> paymentChannelQueryService)
    {
        var allPaymentMethodChannels = queryService.QueryAsync(new AllPaymentMethodChannelQuery());
        var allPaymentMethods = await paymentMethodQueryService
            .QueryAsync(new PaymentMethodQuery())
            .ToDictionaryAsync(pm => pm.Id)
            .ConfigureAwait(false);
        var allPaymentChannels = await paymentChannelQueryService
            .QueryAsync(new PaymentChannelQuery())
            .ToDictionaryAsync(pc => pc.Id)
            .ConfigureAwait(false);

        return new GenericResponse<AllPaymentMethodChannelsResponse>
        {
            Valid = true,
            Data = new AllPaymentMethodChannelsResponse
            {
                AllData = allPaymentMethodChannels
                    .Select(pmc => new PaymentMethodChannelResponse
                    {
                        Id = pmc.Id,
                        PaymentId = pmc.PaymentId,
                        MethodId = pmc.MethodId,
                        ChannelId = pmc.ChannelId
                    })
                    .ToEnumerable(),
                Methods = allPaymentMethodChannels
                    .GroupBy(pmc => pmc.MethodId)
                    .SelectAwait(async g => await g.FirstAsync().ConfigureAwait(false))
                    .Select(pmc => new PaymentMethodModel
                    {
                        Id = pmc.MethodId,
                        Code = allPaymentMethods[pmc.MethodId].Name
                    })
                    .ToEnumerable(),
                Channels = allPaymentMethodChannels
                    .GroupBy(pmc => pmc.ChannelId)
                    .SelectAwait(async g => await g.FirstAsync().ConfigureAwait(false))
                    .Select(pmc => new PaymentChannelModel
                    {
                        Id = pmc.ChannelId,
                        Code = allPaymentChannels[pmc.ChannelId].Name
                    })
                    .ToEnumerable(),
            }
        };
    }
}