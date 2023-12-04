using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;

namespace TGS.Apis.BackendOffice.Controllers.PlatformCredit;

[Route("api/[controller]")]
[ApiController]
public class PlatformCreditController(IMediator mediator) : BaseContextController
{
    [HttpGet("{id}/paymentRecords")]
    [Description("Get payment records")]
    public Task<PagedResponse<PlatformCreditPaymentModel>> GetPaymentRecordsAsync(Guid id,
        [FromQuery] GetPlatformCreditPaymentRequest request)
    {
        request.PlatformCreditId = id;
        return mediator.Send(request);
    }

    [HttpGet]
    [Description("Get platform credits")]
    public Task<PagedResponse<PlatformCreditModel>> GetPlatformCreditsAsync(
        [FromQuery] GetPlatformCreditsRequest request) =>
        mediator.Send(request);

    [HttpPut("{id}/pay/{amount}")]
    [Description("Update a platform credit")]
    public Task<MessageResponse> PayAsync(Guid id, decimal amount) => mediator.Send(new PayRequest(id, amount));

    [HttpPut("{id}/{amount}")]
    [Description("Update a platform credit")]
    public Task<MessageResponse> UpdatePlatformCreditAsync(Guid id, decimal amount)
        => mediator.Send(new UpdatePlatformCreditRequest(id, amount));
}
