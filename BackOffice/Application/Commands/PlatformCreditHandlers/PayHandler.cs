using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TGS.Core.Models.Messages;
using TGS.Services.BackOffice.Application.Models.PlatformCredit;
using TGS.Services.BackOffice.Application.Resources;
using TGS.Services.BackOffice.Infrastructure.DataService;

namespace TGS.Services.BackOffice.Application.Commands.PlatformCreditHandlers;

public class PayHandler(IPlatformCreditService service, IHttpContextAccessor accessor) : Handler<PayRequest, MessageResponse>(accessor)
{
    private readonly IPlatformCreditService _service = service;

    public override async Task<MessageResponse> Handle(PayRequest request,
        CancellationToken cancellationToken)
    {
        var platformCredit = await _service.GetPlatformCreditAsync(q => q.Id == request.Id);

        if (platformCredit == null)
        {
            return CreateInvalidResponse<MessageResponse>(Brand.NotFound);
        }

        await _service.PayAsync(platformCredit, request);

        return CreateValidResponse<MessageResponse>();
    }
}
