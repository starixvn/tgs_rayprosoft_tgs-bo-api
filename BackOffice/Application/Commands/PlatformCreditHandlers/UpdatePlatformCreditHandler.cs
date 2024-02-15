using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Domain;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.PlatformCreditHandlers;

public class UpdatePlatformCreditHandler(
    IPlatformCreditService service,
    IHttpContextAccessor accessor)
    : Handler<UpdatePlatformCreditRequest, MessageResponse>(accessor)
{
    private readonly IPlatformCreditService _service = service;

    public override async Task<MessageResponse> Handle(UpdatePlatformCreditRequest request,
        CancellationToken cancellationToken)
    {
        var brandQuota = await _service.GetPlatformCreditAsync(q => q.Id == request.Id);

        if (brandQuota == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        await _service.UpdatePlatformCreditAsync(brandQuota, request);

        return CreateValidResponse<MessageResponse>();
    }
}
